using System.Drawing;
using System.Drawing.Imaging;

namespace SotnKhaosTools.RandoTracker.Interfaces
{
	public interface IGraphics
	{
		SizeF MeasureString(string text, Font font);
		void DrawString(string s, Font font, Brush brush, float x, float y);
		void Clear(Color color);
		void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes attributes);
		void DrawImage(Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit);
	}
}
