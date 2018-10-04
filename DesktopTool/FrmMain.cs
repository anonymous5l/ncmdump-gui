using DesktopTool.Properties;
using DirectUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopTool
{
    public partial class FrmMain : DirectForm
    {
        private const string NCM_FILE_EXTENSION = ".ncm";
        private const int ITEM_HEIGHT = 32;

        TaskFactory fileProcessFactory = new TaskFactory();

        private List<NeteaseCrypto> _files = new List<NeteaseCrypto>();
        private bool _draging = false;
        private ParallelLoopResult _parallelResult;


        public FrmMain()
        {
            InitializeComponent();
            AllowDrop = true;
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.DrawImage(Resources.shadow, new Rectangle(0, 0, 512, 352));

            using (SolidBrush brush = new SolidBrush(Color.FromArgb(230, 56, 56, 56)))
            {
                e.Graphics.FillRectangle(brush, ClientArea);
            }

            if (_draging)
            {
                e.Graphics.DrawImage(Resources.drag, ClientArea);
            }

            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            using (SolidBrush brush = new SolidBrush(Color.White))
            {
                using (StringFormat sf = StringFormat.GenericDefault)
                {
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Center;
                    sf.FormatFlags = StringFormatFlags.NoWrap;
                    sf.Trimming = StringTrimming.EllipsisCharacter;
                    if (_files.Count <= 0)
                    {
                        e.Graphics.DrawString(Resources.Normal, Font, brush, ClientArea, sf);
                    }
                }
            }

            int _offsetY = 0;

            // graphics item
            for (int i = 0; i < _files.Count; i++)
            {
                NeteaseCrypto f = _files[i];

                RectangleF imgRect = new RectangleF(ClientArea.X, ClientArea.Y + _offsetY, ITEM_HEIGHT, ITEM_HEIGHT);

                float fpw = (ClientArea.Width - imgRect.Width) / 100f;

                RectangleF fillProgress = new RectangleF(imgRect.Right, ClientArea.Y + _offsetY, (float)f.Progress * fpw, ITEM_HEIGHT);

                RectangleF nameRect = new RectangleF(imgRect.Right + 5, ClientArea.Y + _offsetY, ClientArea.Width / 2.8f, ITEM_HEIGHT);
                RectangleF artistRect = new RectangleF(nameRect.Right, ClientArea.Y + _offsetY, nameRect.Width, ITEM_HEIGHT);
                RectangleF progressRect = new RectangleF(artistRect.Right, ClientArea.Y + _offsetY, ClientArea.Width - artistRect.Right, ITEM_HEIGHT);
                using (HightQualityGraphics _ = new HightQualityGraphics(e.Graphics))
                {
                    e.Graphics.DrawImage(f.Cover, imgRect);
                }

                using (SolidBrush brush = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(brush, fillProgress);
                }

                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    using (StringFormat sf = StringFormat.GenericTypographic)
                    {
                        sf.LineAlignment = StringAlignment.Center;
                        sf.FormatFlags = StringFormatFlags.NoWrap;
                        sf.Trimming = StringTrimming.EllipsisCharacter;
                        e.Graphics.DrawString(f.Name, Font, brush, nameRect, sf);

                        e.Graphics.DrawString(String.Join("/", f.Artist), Font, brush, artistRect, sf);

                        e.Graphics.DrawString(string.Format("{0:F}%", f.Progress), Font, brush, progressRect, sf);
                    }
                }

                _offsetY += ITEM_HEIGHT;
            }
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);
            if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
            {
                drgevent.Effect = DragDropEffects.Move;
                _draging = true;
                Invalidate();
            }
        }

        protected async override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);
            OnDragLeave(drgevent);

            var obj = drgevent.Data.GetData(DataFormats.FileDrop);

            if (obj is string[])
            {
                string[] files = (string[])obj;

                foreach (string file in files)
                {
                    var _processing = new FileInfo(file);
                    Text = string.Format(Resources.Processing, _processing.Name);
                    Invalidate();
                    if (!_processing.Attributes.HasFlag(FileAttributes.Directory))
                    {
                        if (_processing.Extension.ToLower() == NCM_FILE_EXTENSION)
                        {
                            var fs = _processing.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
                            await fileProcessFactory.StartNew(new Action(() =>
                            {
                                try
                                {
                                    NeteaseCrypto neteaseFile = new NeteaseCrypto(fs);
                                    _files.Add(neteaseFile);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(string.Format("Error while processing file:\n\t{0}\n\t{1}", _processing.FullName, ex.Message));
                                }
                            }));
                        }
                    }
                }

                Text = Resources.Title;
                Invalidate();

                uiTimer.Start();
                _parallelResult = Parallel.ForEach(_files, (e) =>
                {
                    try
                    {
                        e.Dump();
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                });
            }
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);
            if (_draging)
            {
                _draging = false;
                Invalidate();
            }
        }

        private void uiTimer_Tick(object sender, EventArgs e)
        {
            if (!_parallelResult.IsCompleted)
            {
                Invalidate();
            }
            else
            {
                uiTimer.Stop();
                Invalidate();
                MessageBox.Show("All done!");
                _files.Clear();
                Invalidate();
            }
        }
    }
}
