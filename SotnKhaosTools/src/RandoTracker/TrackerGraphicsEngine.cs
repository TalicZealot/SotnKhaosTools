﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SotnKhaosTools.Configuration.Interfaces;
using SotnKhaosTools.Constants;
using SotnKhaosTools.RandoTracker.Interfaces;
using SotnKhaosTools.RandoTracker.Models;

namespace SotnKhaosTools.RandoTracker
{
	internal sealed class TrackerGraphicsEngine : ITrackerGraphicsEngine
	{
		private const int TextPadding = 5;
		private const int LabelOffset = 50;
		private const int ImageSize = 14;
		private const int CellPadding = 2;
		private const int Columns = 8;
		private const int SeedInfoFontSize = 18;
		private const int CellSize = ImageSize + CellPadding;
		private const double PixelPerfectSnapMargin = 0.18;
		private Color DefaultBackground = Color.FromArgb(17, 00, 17);

		private IGraphics formGraphics;
		private readonly IToolConfig toolConfig;

		private List<Relic>? relics;
		private List<Item>? progressionItems;
		private List<Item>? thrustSwords;

		private List<Bitmap> relicImages = new List<Bitmap>();
		private List<Bitmap> progressionItemImages = new List<Bitmap>();
		private Bitmap? thrustSwordImage;

		private List<Rectangle> relicSlots = new List<Rectangle>();
		private List<Rectangle> vladRelicSlots = new List<Rectangle>();
		private List<Rectangle> progressionItemSlots = new List<Rectangle>();

		private bool initialized = false;
		private float scale = 1;
		private int progressionRelics = 0;
		private bool vladProgression = true;
		private ColorMatrix greyscaleColorMatrix = new ColorMatrix(
			new float[][]
			{
				new float[] {.1f, .1f, .1f, 0, 0},
				new float[] {.3f, .3f, .3f, 0, 0},
				new float[] {.1f, .1f, .1f, 0, 0},
				new float[] {0, 0, 0, 1, 0},
				new float[] {0, 0, 0, 0, 1}
			});

		public TrackerGraphicsEngine(IGraphics formGraphics, IToolConfig toolConfig)
		{
			if (formGraphics is null) throw new ArgumentNullException(nameof(formGraphics));
			if (toolConfig is null) throw new ArgumentNullException(nameof(toolConfig));
			this.formGraphics = formGraphics;
			this.toolConfig = toolConfig;
		}

		public bool Refreshed { get; set; }

		public void InitializeItems(List<Relic> relics, List<Item> progressionItems, List<Item> thrustSwords)
		{
			if (relics is null) throw new ArgumentNullException(nameof(relics));
			if (progressionItems is null) throw new ArgumentNullException(nameof(progressionItems));
			if (thrustSwords is null) throw new ArgumentNullException(nameof(thrustSwords));
			this.relics = relics;
			this.progressionItems = progressionItems;
			this.thrustSwords = thrustSwords;

			foreach (var relic in relics)
			{
				if (relic.Progression)
				{
					progressionRelics++;
				}
			}

			vladProgression = relics[25].Progression;

			LoadImages();

			initialized = true;
		}

		public void SetProgression()
		{
			progressionRelics = 0;

			foreach (var relic in relics)
			{
				if (relic.Progression)
				{
					progressionRelics++;
				}
			}
			vladProgression = relics[25].Progression;
		}

		public void ChangeGraphics(IGraphics formGraphics)
		{
			if (formGraphics is null) throw new ArgumentNullException(nameof(formGraphics));
			this.formGraphics = formGraphics;
		}

		public void CalculateGrid(int width, int height)
		{
			int adjustedColumns = (int) (Columns * (((float) width / (float) height)));
			if (adjustedColumns < 5)
			{
				adjustedColumns = 5;
			}

			int relicCount = 25;
			if (toolConfig.Tracker.ProgressionRelicsOnly)
			{
				relicCount = progressionRelics - 5;
			}

			int normalRelicRows = (int) Math.Ceiling((float) (relicCount + 1) / (float) adjustedColumns);

			float cellsPerColumn = (float) (height - (LabelOffset * 2)) / ((CellSize * (2 + normalRelicRows)));
			float cellsPerRow = (float) (width - (CellPadding * 5)) / ((CellSize * adjustedColumns));
			scale = cellsPerColumn <= cellsPerRow ? cellsPerColumn : cellsPerRow;

			double roundedScale = Math.Floor(scale);

			if (scale - roundedScale < PixelPerfectSnapMargin)
			{
				scale = (float) roundedScale;
			}

			relicSlots = new List<Rectangle>();
			vladRelicSlots = new List<Rectangle>();
			progressionItemSlots = new List<Rectangle>();


			int row = 0;
			int col = 0;

			for (int i = 0; i < relicCount + 1; i++)
			{
				if (col == adjustedColumns)
				{
					row++;
					col = 0;
				}
				relicSlots.Add(new Rectangle((int) (CellPadding + (col * (ImageSize + CellPadding) * scale)), LabelOffset + (int) (row * (ImageSize + CellPadding) * scale), (int) (ImageSize * scale), (int) (ImageSize * scale)));
				col++;
			}

			if (vladProgression)
			{
				row++;
				col = 0;
				for (int i = 0; i < 6; i++)
				{
					vladRelicSlots.Add(new Rectangle((int) (CellPadding + (col * (ImageSize + CellPadding) * scale)), LabelOffset + (int) (row * (ImageSize + CellPadding) * scale), (int) (ImageSize * scale), (int) (ImageSize * scale)));
					col++;
				}
			}

			row++;
			col = 0;
			for (int i = 0; i < 5; i++)
			{
				progressionItemSlots.Add(new Rectangle((int) (CellPadding + (col * (ImageSize + CellPadding) * scale)), LabelOffset + (int) (row * (ImageSize + CellPadding) * scale), (int) (ImageSize * scale), (int) (ImageSize * scale)));
				col++;
			}
		}

		public void DrawSeedInfo(string seedInfo)
		{
			if (seedInfo is null) throw new ArgumentNullException(nameof(seedInfo));
			if (seedInfo == String.Empty) throw new ArgumentException("Parameter seedInfo is empty!");

			int fontSize = SeedInfoFontSize;
			while (formGraphics.MeasureString(seedInfo, new Font("Tahoma", fontSize)).Width > (toolConfig.Tracker.Width - (TextPadding * 3)))
			{
				fontSize--;
			}

			formGraphics.DrawString(seedInfo, new Font("Tahoma", fontSize), new SolidBrush(Color.White), TextPadding, TextPadding);
		}

		public void Render()
		{
			if (!initialized)
			{
				return;
			}

			if (toolConfig.Tracker.GridLayout)
			{
				GridRender();
			}
			else
			{
				CollectedRender();
			}

			Refreshed = true;
		}

		private void GridRender()
		{
			formGraphics.Clear(DefaultBackground);
			ImageAttributes greyscaleAttributes = new ImageAttributes();
			greyscaleAttributes.SetColorMatrix(greyscaleColorMatrix);

			int normalRelicCount = 0;
			for (int i = 0; i < 25; i++)
			{
				if (relics[i].Collected && ((toolConfig.Tracker.ProgressionRelicsOnly && relics[i].Progression) || !toolConfig.Tracker.ProgressionRelicsOnly))
				{
					formGraphics.DrawImage(relicImages[i], relicSlots[normalRelicCount], 0, 0, relicImages[i].Width, relicImages[i].Height, GraphicsUnit.Pixel);
					normalRelicCount++;
				}
				else if ((toolConfig.Tracker.ProgressionRelicsOnly && relics[i].Progression) || !toolConfig.Tracker.ProgressionRelicsOnly)
				{
					formGraphics.DrawImage(relicImages[i], relicSlots[normalRelicCount], 0, 0, relicImages[i].Width, relicImages[i].Height, GraphicsUnit.Pixel, greyscaleAttributes);
					normalRelicCount++;
				}
			}
			var thrustSword = thrustSwords.Where(x => x.Status).FirstOrDefault();
			if (thrustSword != null && thrustSwordImage != null)
			{
				formGraphics.DrawImage(thrustSwordImage, relicSlots[normalRelicCount], 0, 0, thrustSwordImage.Width, thrustSwordImage.Height, GraphicsUnit.Pixel);
			}
			else if (thrustSwordImage != null)
			{
				formGraphics.DrawImage(thrustSwordImage, relicSlots[normalRelicCount], 0, 0, thrustSwordImage.Width, thrustSwordImage.Height, GraphicsUnit.Pixel, greyscaleAttributes);
			}

			if (vladProgression)
			{
				for (int i = 25; i < relics.Count; i++)
				{
					if (relics[i].Collected)
					{
						formGraphics.DrawImage(relicImages[i], vladRelicSlots[i - 25], 0, 0, relicImages[i].Width, relicImages[i].Height, GraphicsUnit.Pixel);
					}
					else
					{
						formGraphics.DrawImage(relicImages[i], vladRelicSlots[i - 25], 0, 0, relicImages[i].Width, relicImages[i].Height, GraphicsUnit.Pixel, greyscaleAttributes);
					}
				}
			}

			for (int i = 0; i < progressionItems.Count; i++)
			{
				if (progressionItems[i].Status)
				{
					formGraphics.DrawImage(progressionItemImages[i], progressionItemSlots[i], 0, 0, progressionItemImages[i].Width, progressionItemImages[i].Height, GraphicsUnit.Pixel);
				}
				else
				{
					formGraphics.DrawImage(progressionItemImages[i], progressionItemSlots[i], 0, 0, progressionItemImages[i].Width, progressionItemImages[i].Height, GraphicsUnit.Pixel, greyscaleAttributes);
				}
			}
		}

		private void CollectedRender()
		{
			formGraphics.Clear(DefaultBackground);

			int normalRelicCount = 0;
			for (int i = 0; i < 25; i++)
			{
				if (relics[i].Collected && ((toolConfig.Tracker.ProgressionRelicsOnly && relics[i].Progression) || !toolConfig.Tracker.ProgressionRelicsOnly))
				{
					formGraphics.DrawImage(relicImages[i], relicSlots[normalRelicCount], 0, 0, relicImages[i].Width, relicImages[i].Height, GraphicsUnit.Pixel);
					normalRelicCount++;
				}
			}
			var thrustSword = thrustSwords.Where(x => x.Status).FirstOrDefault();
			if (thrustSword != null && thrustSwordImage != null)
			{
				formGraphics.DrawImage(thrustSwordImage, relicSlots[normalRelicCount], 0, 0, thrustSwordImage.Width, thrustSwordImage.Height, GraphicsUnit.Pixel);
			}

			if (vladProgression)
			{
				int vladRelicCount = 0;
				for (int i = 25; i < relics.Count; i++)
				{
					if (relics[i].Collected)
					{
						formGraphics.DrawImage(relicImages[i], vladRelicSlots[vladRelicCount], 0, 0, relicImages[i].Width, relicImages[i].Height, GraphicsUnit.Pixel);
						vladRelicCount++;
					}
				}
			}

			int progressionItemCount = 0;
			for (int i = 0; i < progressionItems.Count; i++)
			{
				if (progressionItems[i].Status)
				{
					formGraphics.DrawImage(progressionItemImages[i], progressionItemSlots[progressionItemCount], 0, 0, progressionItemImages[i].Width, progressionItemImages[i].Height, GraphicsUnit.Pixel);
					progressionItemCount++;
				}
			}
		}

		private void LoadImages()
		{
			foreach (var relic in Constants.Paths.RelicImages)
			{
				relicImages.Add(new Bitmap(Image.FromFile(relic.Value)));
			}

			foreach (var item in progressionItems)
			{
				progressionItemImages.Add(new Bitmap(Image.FromFile(Paths.ImagesPath + item.Name + ".png")));
			}

			thrustSwordImage = new Bitmap(Image.FromFile(Paths.ImagesPath + "Claymore.png"));
		}

	}
}