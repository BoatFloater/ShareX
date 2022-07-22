﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2022 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using ShareX.HelpersLib;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShareX
{
    public partial class PinToScreenForm : Form
    {
        public Image Image { get; private set; }

        private int imageScale = 100;

        public int ImageScale
        {
            get
            {
                return imageScale;
            }
            set
            {
                int newImageScale = value.Clamp(10, 500);

                if (imageScale != newImageScale)
                {
                    imageScale = newImageScale;

                    ScaleImage(imageScale);
                }
            }
        }

        private Image buffer;

        private PinToScreenForm()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            MouseWheel += PinToScreenForm_MouseWheel;
        }

        public PinToScreenForm(Image image) : this()
        {
            Image = image;

            LoadImage();
        }

        public static void PinToScreen(Image image)
        {
            PinToScreenForm form = new PinToScreenForm(image);
            form.Show();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                Image?.Dispose();
                buffer?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void LoadImage()
        {
            Image imageCopy = Image.CloneSafe();
            LoadImage(imageCopy);
        }

        private void LoadImage(Image image)
        {
            buffer?.Dispose();
            buffer = image;
            Invalidate();

            AutoSizeForm();
        }

        private void ScaleImage(int scale)
        {
            float scaleFactor = scale / 100f;

            Image imageCopy = Image.CloneSafe();
            Image imageScaled = ImageHelpers.ResizeImage((Bitmap)imageCopy, (int)(Image.Width * scaleFactor), (int)(Image.Height * scaleFactor));
            LoadImage(imageScaled);
        }

        private void AutoSizeForm(bool keepCenterLocation = true)
        {
            if (buffer != null)
            {
                Size previousSize = Size;
                Size newSize = buffer.Size;
                Point newLocation = Location;

                if (keepCenterLocation)
                {
                    Point locationOffset = new Point((previousSize.Width - newSize.Width) / 2, (previousSize.Height - newSize.Height) / 2);
                    newLocation = new Point(newLocation.X + locationOffset.X, newLocation.Y + locationOffset.Y);
                }

                NativeMethods.SetWindowPos(Handle, (IntPtr)SpecialWindowHandles.HWND_TOPMOST, newLocation.X, newLocation.Y,
                    newSize.Width, newSize.Height, SetWindowPosFlags.SWP_NOACTIVATE);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (buffer != null)
            {
                g.DrawImage(buffer, 0, 0);
            }
        }

        private void PinToScreenForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                NativeMethods.ReleaseCapture();
                NativeMethods.SendMessage(Handle, (uint)WindowsMessages.NCLBUTTONDOWN, (IntPtr)WindowHitTestRegions.HTCAPTION, IntPtr.Zero);
            }
        }

        private void PinToScreenForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Close();
            }
        }

        private void PinToScreenForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                ImageScale += 10;
            }
            else if (e.Delta < 0)
            {
                ImageScale -= 10;
            }
        }
    }
}