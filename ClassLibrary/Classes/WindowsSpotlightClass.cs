using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using static System.Net.Mime.MediaTypeNames;

namespace ClassLibrary.Classes
{
	internal class WindowsSpotlightClass
	{
		[JsonProperty("image_fullscreen_001_landscape")]
		public ImageFullscreenLandscapeClass ImageFullscreenLandscape { get; set; }
		[JsonProperty("image_fullscreen_001_portrait")]
		public ImageFullscreenPortraitClass ImageFullscreenPortrait { get; set; }
		[JsonProperty("hs1_title_text")]
		public HsTitleTextClass HsTitleText { get; set; }
		[JsonProperty("title_text")]
		public TitleTextClass TitleText { get; set; }
		public override string ToString()
		{
			return string.Format($"WindowsSpotlight_item:\r\n\timage_fullscreen_001_landscape:{ImageFullscreenLandscape}\r\n\timage_fullscreen_001_portrait:{ImageFullscreenPortrait}\r\n\ths1_title_text:{HsTitleText}\r\n\ttitle_text:{TitleText}");
		}
	}
	internal class ImageFullscreenLandscapeClass
	{
		[JsonProperty("t")]
		public string Type { get; set; }
		[JsonProperty("w")]
		public string Width { get; set; }
		[JsonProperty("h")]
		public string Height { get; set; }
		[JsonProperty("u")]
		public string DownloadUrl { get; set; }
		[JsonProperty("sha256")]
		public string SHA256 { get; set; }
		[JsonProperty("fileSize")]
		public string FileSize { get; set; }
		public override string ToString()
		{
			return string.Format($"image_fullscreen_001_landscape:\r\n\tt:{Type}\r\n\tw:{Width}\r\n\th:{Height}\r\n\tu:{DownloadUrl}\r\n\tsha256:{SHA256}\r\n\tfileSize:{FileSize}");
		}
	}

	internal class ImageFullscreenPortraitClass
	{
		[JsonProperty("t")]
		public string Type { get; set; }
		[JsonProperty("w")]
		public string Width { get; set; }
		[JsonProperty("h")]
		public string Height { get; set; }
		[JsonProperty("u")]
		public string DownloadUrl { get; set; }
		[JsonProperty("sha256")]
		public string SHA256 { get; set; }
		[JsonProperty("fileSize")]
		public string FileSize { get; set; }
		public override string ToString()
		{
			return string.Format($"image_fullscreen_001_portrait:\r\n\tt:{Type}\r\n\tw:{Width}\r\n\th:{Height}\r\n\tu:{DownloadUrl}\r\n\tsha256:{SHA256}\r\n\tfileSize:{FileSize}");
		}
	}

	internal class HsTitleTextClass
	{
		[JsonProperty("t")]
		public string Type { get; set; }
		[JsonProperty("tx")]
		public string Text { get; set; }
		public override string ToString()
		{
			return string.Format($"hs1_title_text:\r\n\tt:{Type}\r\n\ttx:{Text}");
		}
	}

	internal class TitleTextClass
	{
		[JsonProperty("t")]
		public string Type { get; set; }
		[JsonProperty("tx")]
		public string Text { get; set; }
		public override string ToString()
		{
			return string.Format($"title_text:\r\n\tt:{Type}\r\n\ttx:{Text}");
		}
	}

}
