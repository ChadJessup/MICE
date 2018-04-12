using MICE.Common.Misc;
using MICE.Nintendo;
using MICE.Nintendo.Components;
using MICE.Nintendo.Loaders;
using Ninject;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace MICE.TestApp
{
    public partial class Form1 : Form
    {
        private NES nes;
        private Bitmap bitmap;
        private Graphics graphics;
        private IKernel injectionKernel;

        public Form1()
        {
            this.InitializeComponent();

            this.injectionKernel = new StandardKernel(new NintendoModule());

            this.graphics = this.CreateGraphics();
            this.graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            this.bitmap = new Bitmap(256, 240, PixelFormat.Format8bppIndexed);
            ColorPalette palette = this.bitmap.Palette;

            // TODO: Move this to PaletteHandler
            palette.Entries[0x0] = Color.FromArgb(84, 84, 84);
            palette.Entries[0x1] = Color.FromArgb(0, 30, 116);
            palette.Entries[0x2] = Color.FromArgb(8, 16, 144);
            palette.Entries[0x3] = Color.FromArgb(48, 0, 136);
            palette.Entries[0x4] = Color.FromArgb(68, 0, 100);
            palette.Entries[0x5] = Color.FromArgb(92, 0, 48);
            palette.Entries[0x6] = Color.FromArgb(84, 4, 0);
            palette.Entries[0x7] = Color.FromArgb(60, 24, 0);
            palette.Entries[0x8] = Color.FromArgb(32, 42, 0);
            palette.Entries[0x9] = Color.FromArgb(8, 58, 0);
            palette.Entries[0xa] = Color.FromArgb(0, 64, 0);
            palette.Entries[0xb] = Color.FromArgb(0, 60, 0);
            palette.Entries[0xc] = Color.FromArgb(0, 50, 60);
            palette.Entries[0xd] = Color.FromArgb(0, 0, 0);
            palette.Entries[0xe] = Color.FromArgb(0, 0, 0);
            palette.Entries[0xf] = Color.FromArgb(0, 0, 0);
            palette.Entries[0x10] = Color.FromArgb(152, 150, 152);
            palette.Entries[0x11] = Color.FromArgb(8, 76, 196);
            palette.Entries[0x12] = Color.FromArgb(48, 50, 236);
            palette.Entries[0x13] = Color.FromArgb(92, 30, 228);
            palette.Entries[0x14] = Color.FromArgb(136, 20, 176);
            palette.Entries[0x15] = Color.FromArgb(160, 20, 100);
            palette.Entries[0x16] = Color.FromArgb(152, 34, 32);
            palette.Entries[0x17] = Color.FromArgb(120, 60, 0);
            palette.Entries[0x18] = Color.FromArgb(84, 90, 0);
            palette.Entries[0x19] = Color.FromArgb(40, 114, 0);
            palette.Entries[0x1a] = Color.FromArgb(8, 124, 0);
            palette.Entries[0x1b] = Color.FromArgb(0, 118, 40);
            palette.Entries[0x1c] = Color.FromArgb(0, 102, 120);
            palette.Entries[0x1d] = Color.FromArgb(0, 0, 0);
            palette.Entries[0x1e] = Color.FromArgb(0, 0, 0);
            palette.Entries[0x1f] = Color.FromArgb(0, 0, 0);
            palette.Entries[0x20] = Color.FromArgb(236, 238, 236);
            palette.Entries[0x21] = Color.FromArgb(76, 154, 236);
            palette.Entries[0x22] = Color.FromArgb(120, 124, 236);
            palette.Entries[0x23] = Color.FromArgb(176, 98, 236);
            palette.Entries[0x24] = Color.FromArgb(228, 84, 236);
            palette.Entries[0x25] = Color.FromArgb(236, 88, 180);
            palette.Entries[0x26] = Color.FromArgb(236, 106, 100);
            palette.Entries[0x27] = Color.FromArgb(212, 136, 32);
            palette.Entries[0x28] = Color.FromArgb(160, 170, 0);
            palette.Entries[0x29] = Color.FromArgb(116, 196, 0);
            palette.Entries[0x2a] = Color.FromArgb(76, 208, 32);
            palette.Entries[0x2b] = Color.FromArgb(56, 204, 108);
            palette.Entries[0x2c] = Color.FromArgb(56, 180, 204);
            palette.Entries[0x2d] = Color.FromArgb(60, 60, 60);
            palette.Entries[0x2e] = Color.FromArgb(0, 0, 0);
            palette.Entries[0x2f] = Color.FromArgb(0, 0, 0);
            palette.Entries[0x30] = Color.FromArgb(236, 238, 236);
            palette.Entries[0x31] = Color.FromArgb(168, 204, 236);
            palette.Entries[0x32] = Color.FromArgb(188, 188, 236);
            palette.Entries[0x33] = Color.FromArgb(212, 178, 236);
            palette.Entries[0x34] = Color.FromArgb(236, 174, 236);
            palette.Entries[0x35] = Color.FromArgb(236, 174, 212);
            palette.Entries[0x36] = Color.FromArgb(236, 180, 176);
            palette.Entries[0x37] = Color.FromArgb(228, 196, 144);
            palette.Entries[0x38] = Color.FromArgb(204, 210, 120);
            palette.Entries[0x39] = Color.FromArgb(180, 222, 120);
            palette.Entries[0x3a] = Color.FromArgb(168, 226, 144);
            palette.Entries[0x3b] = Color.FromArgb(152, 226, 180);
            palette.Entries[0x3c] = Color.FromArgb(160, 214, 228);
            palette.Entries[0x3d] = Color.FromArgb(160, 162, 160);
            palette.Entries[0x3e] = Color.FromArgb(0, 0, 0);
            palette.Entries[0x3f] = Color.FromArgb(0, 0, 0);

            this.bitmap.Palette = palette;
        }

        private void Go()
        {
            this.nes = this.injectionKernel.Get<NES>();

            this.nes.InputHandler.SetController1(new KeyboardController(this.nes.InputHandler, new Range(0x4016, 0x4016), "Control Input 1"));
            //this.nes.InputHandler.SetController2(new KeyboardController(new Range(0x4017, 0x4017), "Control Input 2"));

            //var cartridge = NESLoader.CreateCartridge(@"C:\Emulators\NES\Games\World\Donkey Kong (JU).nes");
            var cartridge = NESLoader.CreateCartridge(@"C:\Emulators\NES\Games\Super Mario Bros.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\Emulators\NES\Games\USA\Legend of Zelda, The (U) (PRG 1).nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\Emulators\NES\Games\USA\Bionic Commando (U).nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\Emulators\NES\Games\USA\Mega Man (U).nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\Emulators\NES\Games\USA\Slalom (U).nes");

            // Controller Tests
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\read_joy3\test_buttons.nes");

            // CPU Tests
            // PASSING
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\nes_instr_test\rom_singles\01-implied.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\nes_instr_test\rom_singles\02-immediate.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\nes_instr_test\rom_singles\03-zero_page.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\nes_instr_test\rom_singles\04-zp_xy.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\nes_instr_test\rom_singles\05-absolute.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\nes_instr_test\rom_singles\06-abs_xy.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\nes_instr_test\rom_singles\07-ind_x.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\nes_instr_test\rom_singles\08-ind_y.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\nes_instr_test\rom_singles\09-branches.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\nes_instr_test\rom_singles\10-stack.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\nes_instr_test\rom_singles\11-special.nes");

            // NMI Tests
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\vbl_nmi_timing\1.frame_basics.nes");

            // FAILING
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\blargg_nes_cpu_test5\official.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\cpu_timing_test6\cpu_timing_test.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\oam_read\oam_read.nes");


            // Goes white after a bit, need to look into...duplicate set though, i think?
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\instr_test-v3\rom_singles\01-implied.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\instr_test-v3\rom_singles\02-immediate.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\instr_test-v3\rom_singles\03-zero_page.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\instr_test-v3\rom_singles\04-zp_xy.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\instr_test-v3\rom_singles\05-absolute.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\instr_test-v3\rom_singles\06-abs_xy.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\instr_test-v3\rom_singles\07-ind_x.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\instr_test-v3\rom_singles\08-ind_y.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\instr_test-v3\rom_singles\09-branches.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\instr_test-v3\rom_singles\10-stack.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\instr_test-v3\rom_singles\11-jmp_jsr.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\instr_test-v3\rom_singles\12-rts.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\instr_test-v3\rom_singles\13-rti.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\instr_test-v3\rom_singles\14-brk.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\instr_test-v3\rom_singles\15-special.nes");

            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\blargg_nes_cpu_test5\official.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\nestest.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\instr_test-v3\official_only.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\cpu_timing_test6\cpu_timing_test.nes");

            // PPU Tests
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\scanline\scanline.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\scanline-a1\scanline.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\scrolltest\scroll.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\blargg_ppu_tests_2005.09.15b\palette_ram.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\blargg_ppu_tests_2005.09.15b\power_up_palette.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\blargg_ppu_tests_2005.09.15b\sprite_ram.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\blargg_ppu_tests_2005.09.15b\vbl_clear_time.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\blargg_ppu_tests_2005.09.15b\vram_access.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\full_palette\full_palette.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\scrolltest\scroll.nes");

            // needs CNROM
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\cpu_dummy_reads\cpu_dummy_reads.nes");

            // Sprites
            // Passing
            // Sprite-0 hits
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\sprite_hit_tests_2005.10.05\01.basics.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\sprite_hit_tests_2005.10.05\02.alignment.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\sprite_hit_tests_2005.10.05\03.corners.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\sprite_hit_tests_2005.10.05\04.flip.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\sprite_hit_tests_2005.10.05\05.left_clip.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\sprite_hit_tests_2005.10.05\06.right_edge.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\sprite_hit_tests_2005.10.05\07.screen_bottom.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\sprite_hit_tests_2005.10.05\08.double_height.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\sprite_hit_tests_2005.10.05\09.timing_basics.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\sprite_hit_tests_2005.10.05\10.timing_order.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\sprite_hit_tests_2005.10.05\11.edge_timing.nes");

            // Sprite overflow
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\sprite_overflow_tests\1.Basics.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\sprite_overflow_tests\2.Details.nes");

            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\sprite_overflow_tests\3.Timing.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\sprite_overflow_tests\4.Obscure.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\sprite_overflow_tests\5.Emulator.nes");


            // Misc
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\nes_instr_test\rom_singles\02-immediate.nes");
            //var cartridge = NESLoader.CreateCartridge(@"C:\src\emulators\nes-test-roms\nes_instr_test\rom_singles\03-zero_page.nes");

            this.nes.LoadCartridge(cartridge);

            this.nes.PowerOn();

            Task.Factory.StartNew(
                () => this.nes.Run(),
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Current);

            var uiDispatcher = Dispatcher.CurrentDispatcher;

            Task.Factory.StartNew(() =>
            {
                DateTime lastTime = DateTime.Now;
                long lastFrame = 0;
                double fps = 0;
                double improveBy = 0;

                while (!this.nes.IsPaused && this.nes.IsPoweredOn)
                {
                    if ((DateTime.Now - lastTime).TotalSeconds >= 1)
                    {
                        fps = nes.CurrentFrame - lastFrame;
                        lastFrame = nes.CurrentFrame;
                        improveBy = 60 / fps;
                        lastTime = DateTime.Now;
                        uiDispatcher.Invoke(() => this.Text = $"Frame: {this.nes.CurrentFrame} FPS: {fps} Improve: {improveBy:F2}x");
                    }

                    this.Draw(this.nes.Screen);
                }
            },
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Current);
        }

        private Rectangle bitmapRectangle = new Rectangle(0, 0, 256, 240);
        private void Draw(byte[] screen)
        {
            BitmapData frame = this.bitmap.LockBits(this.bitmapRectangle, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            //var ptr = frame.Scan0;
            unsafe
            {
                var toSpan = new Span<byte>(frame.Scan0.ToPointer(), screen.Length);
                var fromSpan = new Span<byte>(screen);
                fromSpan.CopyTo(toSpan);
            }

            //Marshal.Copy(screen, 0, ptr, screen.Length);

            this.bitmap.UnlockBits(frame);

            try
            {
                this.graphics.DrawImage(this.bitmap, this.bitmapRectangle);
            }
            catch (ExternalException)
            { }
        }

        private void OnShown(object sender, EventArgs e)
        {
            this.Go();
        }
    }
}
