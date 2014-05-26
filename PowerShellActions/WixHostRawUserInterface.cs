using System;
using System.Management.Automation.Host;

namespace PowerShellActions
{
    public class WixHostRawUserInterface : PSHostRawUserInterface
    {
        public override ConsoleColor BackgroundColor { get; set; }
        public override Size BufferSize { get; set; }
        public override Coordinates CursorPosition { get; set; }
        public override int CursorSize { get; set; }
        public override ConsoleColor ForegroundColor { get; set; }

        public override bool KeyAvailable
        {
            get
            {
                throw new NotImplementedException("KeyAvailable");
            }
        }

        public override Size MaxPhysicalWindowSize
        {
            get
            {
                throw new NotImplementedException("MaxPhysicalWindowSize");
            }
        }

        public override Size MaxWindowSize
        {
            get
            {
                throw new NotImplementedException("MaxWindowSize");
            }
        }

        public override Coordinates WindowPosition { get; set; }
        public override Size WindowSize { get; set; }
        public override string WindowTitle { get; set; }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            throw new NotImplementedException("ReadKey");
        }

        public override void FlushInputBuffer()
        {
        }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
        {
        }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
        {
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle)
        {
            throw new NotImplementedException("GetBufferContents");
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
        {
        }
    }
}