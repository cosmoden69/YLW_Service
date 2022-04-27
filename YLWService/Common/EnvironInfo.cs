/*
 * Created by SharpDevelop.
 * User: hiworld
 * Date: 2011-07-31
 * Time: 오전 9:55
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace MetroSoft.HIS
{
	/// <summary>
	/// 환경정보 Static Class
	/// </summary>
	public class EnvironInfo
	{
		public const string		Product = "MetroSoft.HIS";    	// Product명
		public const int		UdpPort = 15021;		//Udp Msg Port
        const string            LOGIN_IMAGE_NAME = "Login.bmp";
		const string			MAIN_ICON_NAME	 = "MetroSoft.HIS.ico";	
		private static string	imagePath;		// Image Path
        private static IWindowContainer mdiForm = null;
        private static Hashtable openSystemList = new Hashtable();  //Key는 SystemID, value는 MdiForm의 Handle 관리
        private static string currSystemID = string.Empty;
        private static string currSystemName = string.Empty;

		//f
		public static IWindowContainer MdiForm
		{
			get { return mdiForm; }
			set { mdiForm = value; }
		}
		
		public static string ImagePath
		{
			get { return imagePath;}
		}
		
		public static string ClientIP
		{
			get 
			{
				try
				{
					//IP 2개이상 지정된 PC의 경우 최종  IP가 사용하는 IP임
					IPAddress[] ipList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
					if (ipList.Length > 0)
						return ipList[ipList.Length -1].ToString();
					else
						return "UnKnown";
				}
				catch
				{
					return "UnKnown";
				}
			}
		}
		
        // Open된 시스템 정보를 어디까지 관리할지에 따라 변경
        public static Hashtable OpenSystemList
        {
            get { return openSystemList; }
        }

        public static string CurrSystemID
        {
            get { return currSystemID; }
            set { currSystemID = value; }
        }
        
        public static string CurrSystemName
        {
            get { return currSystemName; }
            set { currSystemName = value; }
        }

		static EnvironInfo()
		{
            string mypath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            imagePath = Directory.GetParent(mypath).FullName + @"\Images";
			if (!Directory.Exists(imagePath))
			{
				Directory.CreateDirectory(imagePath);
			}
		}
		
		/// <summary>
		//Login.gif를 사용하는 사용자로그인,비밀번호변경,로그아웃 창의 Image 설정
		public static void SetBackgroundImage(Form form)
		{
			try
			{
				string fileName = EnvironInfo.ImagePath + "\\" + EnvironInfo.LOGIN_IMAGE_NAME;
				if (File.Exists(fileName)) 
				{
					Bitmap backImg = Image.FromFile(fileName) as Bitmap;
					form.BackgroundImage = backImg;
					//Bitmap TransParent (테두리가 TransParent이므로 적용안함)
					Color tColor = backImg.GetPixel(0,0);
                    //backImg.MakeTransparent(tColor);

					//Size 설정 (MaxSize도 고정)
					form.ClientSize = backImg.Size;
					form.MaximumSize = backImg.Size;

					//Form의 Region 설정
                    //form.Region = new Region(CalculateGraphicsPath(backImg));
				}
				
				fileName = EnvironInfo.ImagePath + "\\" + EnvironInfo.MAIN_ICON_NAME;
				if (File.Exists(fileName))
				{
					form.Icon = new Icon(fileName);
				}
			}
			finally{}
		}
		
		private static GraphicsPath CalculateGraphicsPath(Bitmap bitmap)
		{
			GraphicsPath graphicsPath = new GraphicsPath();

			// TransParent Color
			Color tColor = bitmap.GetPixel(1, 1);
			
			//Bitmap의 크기는 400,300
			// 0 ~ 5까지는 TransParent 계산, 6 ~ 294까지는 Rect 적용(TransParent없음)
			// 295 ~height까지 TransParent 계산

			Rectangle rect = Rectangle.Empty;
            int width = bitmap.Width;
            int height = bitmap.Height;

			// Go through all rows (Y axis)
			for(int row = 0; row < 5; row ++)
			{
                graphicsPath.AddRectangle(new Rectangle(5 - row, row, width - (5 - row) * 2, 1));
			}
            graphicsPath.AddRectangle(new Rectangle(0, 5, width, 290));
			for(int row = 295; row < height; row ++)
			{
                graphicsPath.AddRectangle(new Rectangle(height - row, row, width - (height - row) * 2, 1));
			}

			// Return calculated graphics path
			return graphicsPath;
		}

        private static GraphicsPath CalculateGraphicsPath1(Bitmap bitmap)
        {
            GraphicsPath graphicsPath = new GraphicsPath();

            // TransParent Color
            Color tColor = bitmap.GetPixel(0, 0);

            //Bitmap의 크기는 400,300
            // 0 ~ 5까지는 TransParent 계산, 6 ~ 294까지는 Rect 적용(TransParent없음)
            // 295 ~height까지 TransParent 계산

            int colOpaquePixel = 0;
            Rectangle rect = Rectangle.Empty;

            // Go through all rows (Y axis)
            for (int row = 0; row < 5; row++)
            {
                // Reset value
                colOpaquePixel = 0;

                // Go through all columns (X axis)
                for (int col = 0; col < bitmap.Width; col++)
                {
                    if (bitmap.GetPixel(col, row) != tColor)
                    {
                        colOpaquePixel = col;
                        int colNext = col;
                        for (colNext = colOpaquePixel; colNext < bitmap.Width; colNext++)
                            if (bitmap.GetPixel(colNext, row) == tColor)
                                break;
                        graphicsPath.AddRectangle(new Rectangle(colOpaquePixel, row, colNext - colOpaquePixel, 1));
                        rect = new Rectangle(colOpaquePixel, row, colNext - colOpaquePixel, 1);
                        col = colNext;
                    }
                }
            }
            
            graphicsPath.AddRectangle(new Rectangle(0, 5, bitmap.Width, 290));
            for (int row = 295; row < bitmap.Height; row++)
            {
                // Reset value
                colOpaquePixel = 0;

                // Go through all columns (X axis)
                for (int col = 0; col < bitmap.Width; col++)
                {
                    if (bitmap.GetPixel(col, row) != tColor)
                    {
                        colOpaquePixel = col;
                        int colNext = col;
                        for (colNext = colOpaquePixel; colNext < bitmap.Width; colNext++)
                            if (bitmap.GetPixel(colNext, row) == tColor)
                                break;
                        graphicsPath.AddRectangle(new Rectangle(colOpaquePixel, row, colNext - colOpaquePixel, 1));
                        rect = new Rectangle(colOpaquePixel, row, colNext - colOpaquePixel, 1);
                        col = colNext;
                    }
                }
            }

            // Return calculated graphics path
            return graphicsPath;
        }

        //A라는 UserControl 안에 B라는 UserControl을 넣으면 A의 DesignMode는 true, B의 DesignMode는 false가 된다.
        //그래서 아래와 같이 실행중인지 체크하는 메소드를 추가하여 해결..
        public static bool IsInDesigner
        {
            get { return (System.Reflection.Assembly.GetEntryAssembly() == null); }
        }
    }
}
