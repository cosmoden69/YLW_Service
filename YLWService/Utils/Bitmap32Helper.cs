using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace YLWService
{
    /// <summary>
    /// 32비트 비트맵 헬퍼
    /// </summary>
    public class Bitmap32Helper
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////// Field
        ////////////////////////////////////////////////////////////////////////////////////////// Static
        //////////////////////////////////////////////////////////////////////////////// Public

        #region 엠보싱 필터 1 - EmbossingFilter1

        /// <summary>
        /// 엠보싱 필터 1
        /// </summary>
        public static Filter EmbossingFilter1
        {
            get
            {
                return new Filter()
                {
                    Weight = 1,
                    Offset = 127,
                    Kernel = new float[,]
                    {
                        { -1, 0, 0 },
                        {  0, 0, 0 },
                        {  0, 0, 1 },
                    }
                };
            }
        }

        #endregion
        #region 엠보싱 필터 2 - EmbossingFilter2

        /// <summary>
        /// 엠보싱 필터 2
        /// </summary>
        public static Filter EmbossingFilter2
        {
            get
            {
                return new Filter()
                {
                    Weight = 1,
                    Offset = 127,
                    Kernel = new float[,]
                    {
                        { 2,  0,  0 },
                        { 0, -1,  0 },
                        { 0,  0, -1 },
                    }
                };
            }
        }

        #endregion
        #region 엠보싱 필터 3 - EmbossingFilter3

        /// <summary>
        /// 엠보싱 필터 3
        /// </summary>
        public static Filter EmbossingFilter3
        {
            get
            {
                return new Filter()
                {
                    Weight = 1,
                    Offset = 0,
                    Kernel = new float[,]
                    {
                        { 1,  1, -1 },
                        { 1,  1, -1 },
                        { 1, -1, -1 },
                    }
                };
            }
        }

        #endregion
        #region 블러 필터 5×5 가우시안 - BlurFilter5x5Gaussian

        /// <summary>
        /// 블러 필터 5×5 가우시안
        /// </summary>
        public static Filter BlurFilter5x5Gaussian
        {
            get
            {
                Filter filter = new Filter()
                {
                    Offset = 0,
                    Kernel = new float[,]
                    {
                        { 1,  4,  7,  4, 1 },
                        { 4, 16, 26, 16, 4 },
                        { 7, 26, 41, 26, 7 },
                        { 4, 16, 26, 16, 4 },
                        { 1,  4,  7,  4, 1 },
                    }
                };

                filter.Normalize();

                return filter;
            }
        }

        #endregion
        #region 블러 필터 5×5 평균 - BlurFilter5x5Mean

        /// <summary>
        /// 블러 필터 5×5 평균
        /// </summary>
        public static Filter BlurFilter5x5Mean
        {
            get
            {
                Filter filter = new Filter()
                {
                    Offset = 0,
                    Kernel = new float[,]
                    {
                        { 1, 1, 1, 1, 1 },
                        { 1, 1, 1, 1, 1 },
                        { 1, 1, 1, 1, 1 },
                        { 1, 1, 1, 1, 1 },
                        { 1, 1, 1, 1, 1 },
                    }
                };

                filter.Normalize();

                return filter;
            }
        }

        #endregion
        #region 엣지 탐지 필터 좌상단-우하단 - EdgeDetectionFilterULtoLR

        /// <summary>
        /// 엣지 탐지 필터 좌상단-우하단
        /// </summary>
        public static Filter EdgeDetectionFilterULtoLR
        {
            get
            {
                return new Filter()
                {
                    Weight = 1,
                    Offset = 0,
                    Kernel = new float[,]
                    {
                        { -5, 0, 0 },
                        {  0, 0, 0 },
                        {  0, 0, 5 },
                    }
                };
            }
        }

        #endregion
        #region 엣지 탐지 필터 상-하 - EdgeDetectionFilterTopToBottom

        /// <summary>
        /// 엣지 탐지 필터 상-하
        /// </summary>
        public static Filter EdgeDetectionFilterTopToBottom
        {
            get
            {
                return new Filter()
                {
                    Weight = 1,
                    Offset = 0,
                    Kernel = new float[,]
                    {
                        { -1, -1, -1 },
                        {  0,  0,  0 },
                        {  1,  1,  1 },
                    }
                };
            }
        }

        #endregion
        #region 엣지 탐지 필터 좌-우 - EdgeDetectionFilterLeftToRight

        /// <summary>
        /// 엣지 탐지 필터 좌-우
        /// </summary>
        public static Filter EdgeDetectionFilterLeftToRight
        {
            get
            {
                return new Filter()
                {
                    Weight = 1,
                    Offset = 0,
                    Kernel = new float[,]
                    {
                        { -1, 0, 1 },
                        { -1, 0, 1 },
                        { -1, 0, 1 },
                    }
                };
            }
        }

        #endregion
        #region 하이 패스 필터 3×3 - HighPassFilter3x3

        /// <summary>
        /// 하이 패스 필터 3×3
        /// </summary>
        public static Filter HighPassFilter3x3
        {
            get
            {
                return new Filter()
                {
                    Weight = 16,
                    Offset = 127,
                    Kernel = new float[,]
                    {
                        { -1, -2, -1 },
                        { -2, 12, -2 },
                        { -1, -2, -1 },
                    }
                };
            }
        }

        #endregion
        #region 하이 패스 필터 5×5 - HighPassFilter5x5

        /// <summary>
        /// 하이 패스 필터 5×5
        /// </summary>
        public static Filter HighPassFilter5x5
        {
            get
            {
                Filter filter = new Filter()
                {
                    Offset = 127,
                    Kernel = new float[,]
                    {
                        { -1,  -4,  -7,  -4, -1 },
                        { -4, -16, -26, -16, -4 },
                        { -7, -26, -41, -26, -7 },
                        { -4, -16, -26, -16, -4 },
                        { -1,  -4,  -7,  -4, -1 },
                    }
                };

                filter.Normalize();

                filter.Weight = -filter.Weight;

                filter.SetZeroKernel();

                return filter;
            }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////// Instance
        //////////////////////////////////////////////////////////////////////////////// Private

        #region Field

        /// <summary>
        /// 비트맵
        /// </summary>
        private Bitmap bitmap;

        /// <summary>
        /// 비트맵 데이터
        /// </summary>
        private BitmapData bitmapData;

        /// <summary>
        /// 행 바이트 카운트
        /// </summary>
        private int rowByteCount;

        /// <summary>
        /// 이미지 바이트 배열
        /// </summary>
        private byte[] imageByteArray;

        /// <summary>
        /// 잠금 여부
        /// </summary>
        private bool isLocked = false;

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Property
        ////////////////////////////////////////////////////////////////////////////////////////// Public

        #region 비트맵 - Bitmap

        /// <summary>
        /// 비트맵
        /// </summary>
        public Bitmap Bitmap
        {
            get
            {
                return this.bitmap;
            }
        }

        #endregion
        #region 너비 - Width

        /// <summary>
        /// 너비
        /// </summary>
        public int Width
        {
            get
            {
                return this.bitmap.Width;
            }
        }

        #endregion
        #region 높이 - Height

        /// <summary>
        /// 높이
        /// </summary>
        public int Height
        {
            get
            {
                return this.bitmap.Height;
            }
        }

        #endregion
        #region 잠금 여부 - IsLocked

        /// <summary>
        /// 잠금 여부
        /// </summary>
        public bool IsLocked
        {
            get
            {
                return this.isLocked;
            }
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Constructor
        ////////////////////////////////////////////////////////////////////////////////////////// Public

        #region 생성자 - Bitmap32Helper(bitmap)

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="bitmap">비트맵</param>
        public Bitmap32Helper(Bitmap bitmap)
        {
            this.bitmap = bitmap;
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Method
        ////////////////////////////////////////////////////////////////////////////////////////// Public

        #region 비트맵 잠그기 - LockBitmap()

        /// <summary>
        /// 비트맵 잠그기
        /// </summary>
        public void LockBitmap()
        {
            if (this.isLocked)
            {
                return;
            }

            Rectangle boundRectangle = new Rectangle
            (
                0,
                0,
                this.bitmap.Width,
                this.bitmap.Height
            );

            this.bitmapData = this.bitmap.LockBits
            (
                boundRectangle,
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb
            );

            this.rowByteCount = this.bitmapData.Stride;

            int totalByteCount = this.bitmapData.Stride * this.bitmapData.Height;

            this.imageByteArray = new byte[totalByteCount];

            Marshal.Copy(this.bitmapData.Scan0, this.imageByteArray, 0, totalByteCount);

            this.isLocked = true;
        }

        #endregion
        #region 비트맵 잠금 취소하기 - UnlockBitmap()

        /// <summary>
        /// 비트맵 잠금 취소하기
        /// </summary>
        public void UnlockBitmap()
        {
            if (!this.isLocked)
            {
                return;
            }

            int totalByteCount = this.bitmapData.Stride * this.bitmapData.Height;

            Marshal.Copy(this.imageByteArray, 0, this.bitmapData.Scan0, totalByteCount);

            this.bitmap.UnlockBits(this.bitmapData);

            this.imageByteArray = null;

            this.bitmapData = null;

            this.isLocked = false;
        }

        #endregion

        #region 픽셀 구하기 - GetPixel(x, y, red, green, blue, alpha)

        /// <summary>
        /// 픽셀 구하기
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="red">빨강색 채널</param>
        /// <param name="green">녹색 채널</param>
        /// <param name="blue">파랑색 채널</param>
        /// <param name="alpha">투명도 채널</param>
        public void GetPixel(int x, int y, out byte red, out byte green, out byte blue, out byte alpha)
        {
            int i = y * this.bitmapData.Stride + x * 4;

            blue = this.imageByteArray[i++];
            green = this.imageByteArray[i++];
            red = this.imageByteArray[i++];
            alpha = this.imageByteArray[i];
        }

        #endregion
        #region 픽셀 설정하기 - SetPixel(x, y, red, green, blue, alpha)

        /// <summary>
        /// 픽셀 설정하기
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="red">빨강색 채널</param>
        /// <param name="green">녹색 채널</param>
        /// <param name="blue">파랑색 채널</param>
        /// <param name="alpha">투명도 채널</param>
        public void SetPixel(int x, int y, byte red, byte green, byte blue, byte alpha)
        {
            int i = y * this.bitmapData.Stride + x * 4;

            this.imageByteArray[i++] = blue;
            this.imageByteArray[i++] = green;
            this.imageByteArray[i++] = red;
            this.imageByteArray[i] = alpha;
        }

        #endregion

        #region 파랑색 채널 구하기 - GetBlue(x, y)

        /// <summary>
        /// 파랑색 채널 구하기
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>파랑색 채널</returns>
        public byte GetBlue(int x, int y)
        {
            int i = y * this.bitmapData.Stride + x * 4;

            return this.imageByteArray[i];
        }

        #endregion
        #region 파랑색 채널 설정하기 - SetBlue(x, y, blue)

        /// <summary>
        /// 파랑색 채널 설정하기
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="blue">파랑색 채널</param>
        public void SetBlue(int x, int y, byte blue)
        {
            int i = y * this.bitmapData.Stride + x * 4;

            this.imageByteArray[i] = blue;
        }

        #endregion
        #region 녹색 채널 구하기 - GetGreen(x, y)

        /// <summary>
        /// 녹색 채널 구하기
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>녹색 채널</returns>
        public byte GetGreen(int x, int y)
        {
            int i = y * this.bitmapData.Stride + x * 4;

            return this.imageByteArray[i + 1];
        }

        #endregion
        #region 녹색 채널 설정하기 - SetGreen(x, y, green)

        /// <summary>
        /// 녹색 채널 설정하기
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="green">녹색 채널</param>
        public void SetGreen(int x, int y, byte green)
        {
            int i = y * this.bitmapData.Stride + x * 4;

            this.imageByteArray[i + 1] = green;
        }

        #endregion
        #region 빨강색 채널 구하기 - GetRed(x, y)

        /// <summary>
        /// 빨강색 채널 구하기
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>빨강색 채널</returns>
        public byte GetRed(int x, int y)
        {
            int i = y * this.bitmapData.Stride + x * 4;

            return this.imageByteArray[i + 2];
        }

        #endregion
        #region 빨강색 채널 설정하기 - SetRed(x, y, red)

        /// <summary>
        /// 빨강색 채널 설정하기
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="red">빨강색 채널</param>
        public void SetRed(int x, int y, byte red)
        {
            int i = y * this.bitmapData.Stride + x * 4;

            this.imageByteArray[i + 2] = red;
        }

        #endregion
        #region 투명도 채널 구하기 - GetAlpha(x, y)

        /// <summary>
        /// 투명도 채널 구하기
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>투명도 채널</returns>
        public byte GetAlpha(int x, int y)
        {
            int i = y * this.bitmapData.Stride + x * 4;

            return this.imageByteArray[i + 3];
        }

        #endregion
        #region 투명도 채널 설정하기 - SetAlpha(x, y, alpha)

        /// <summary>
        /// 투명도 채널 설정하기
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="alpha">투명도 채널</param>
        public void SetAlpha(int x, int y, byte alpha)
        {
            int i = y * this.bitmapData.Stride + x * 4;

            this.imageByteArray[i + 3] = alpha;
        }

        #endregion

        #region 평균 회색조 적용하기 - ApplyAverageGrayscale()

        /// <summary>
        /// 평균 회색조 적용하기
        /// </summary>
        public void ApplyAverageGrayscale()
        {
            bool lockStatus = this.isLocked;

            LockBitmap();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    byte red;
                    byte green;
                    byte blue;
                    byte alpha;

                    GetPixel(x, y, out red, out green, out blue, out alpha);

                    byte gray = (byte)((red + green + blue) / 3);

                    SetPixel(x, y, gray, gray, gray, alpha);
                }
            }

            if (!lockStatus)
            {
                UnlockBitmap();
            }
        }

        #endregion
        #region 회색조 적용하기 - ApplyGrayscale()

        /// <summary>
        /// 회색조 적용하기
        /// </summary>
        public void ApplyGrayscale()
        {
            bool lockStatus = this.isLocked;

            LockBitmap();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    byte red;
                    byte green;
                    byte blue;
                    byte alpha;

                    GetPixel(x, y, out red, out green, out blue, out alpha);

                    byte gray = (byte)(0.3 * red + 0.5 * green + 0.2 * blue);

                    SetPixel(x, y, gray, gray, gray, alpha);
                }
            }

            if (!lockStatus)
            {
                UnlockBitmap();
            }
        }

        #endregion

        #region 빨강색 채널 지우기 - ClearRed()

        /// <summary>
        /// 빨강색 채널 지우기
        /// </summary>
        public void ClearRed()
        {
            bool lockStatus = this.isLocked;

            LockBitmap();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    SetRed(x, y, 0);
                }
            }

            if (!lockStatus)
            {
                UnlockBitmap();
            }
        }

        #endregion
        #region 녹색 채널 지우기 - ClearGreen()

        /// <summary>
        /// 녹색 채널 지우기
        /// </summary>
        public void ClearGreen()
        {
            bool lockStatus = IsLocked;

            LockBitmap();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    SetGreen(x, y, 0);
                }
            }

            if (!lockStatus)
            {
                UnlockBitmap();
            }
        }

        #endregion
        #region 파랑색 채널 지우기 - ClearBlue()

        /// <summary>
        /// 파랑색 채널 지우기
        /// </summary>
        public void ClearBlue()
        {
            bool lockStatus = IsLocked;

            LockBitmap();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    SetBlue(x, y, 0);
                }
            }

            if (!lockStatus)
            {
                UnlockBitmap();
            }
        }

        #endregion

        #region 반전하기 - Invert()

        /// <summary>
        /// 반전하기
        /// </summary>
        public void Invert()
        {
            bool lockStatus = IsLocked;

            LockBitmap();

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    byte red = (byte)(255 - GetRed(x, y));
                    byte green = (byte)(255 - GetGreen(x, y));
                    byte blue = (byte)(255 - GetBlue(x, y));
                    byte alpha = GetAlpha(x, y);

                    SetPixel(x, y, red, green, blue, alpha);
                }
            }

            if (!lockStatus)
            {
                UnlockBitmap();
            }
        }

        #endregion

        #region 복제하기 - Clone()

        /// <summary>
        /// 복제하기
        /// </summary>
        /// <returns>비트맵 32 헬퍼</returns>
        public Bitmap32Helper Clone()
        {
            bool lockStatus = this.IsLocked;

            this.LockBitmap();

            Bitmap32Helper helper = (Bitmap32Helper)this.MemberwiseClone();

            helper.bitmap = new Bitmap(this.bitmap.Width, this.bitmap.Height);

            helper.isLocked = false;

            if (!lockStatus)
            {
                UnlockBitmap();
            }

            return helper;
        }

        #endregion
        #region 필터 적용하기 - ApplyFilter(filter, lockResult)

        /// <summary>
        /// 필터 적용하기
        /// </summary>
        /// <param name="filter">필터</param>
        /// <param name="lockResult">잠금 결과</param>
        /// <returns>비트맵 32 헬퍼</returns>
        public Bitmap32Helper ApplyFilter(Filter filter, bool lockResult)
        {
            Bitmap32Helper helper = this.Clone();

            bool lockStatus = this.isLocked;

            LockBitmap();

            helper.LockBitmap();

            int xOffset = -(int)(filter.Kernel.GetUpperBound(1) / 2);
            int yOffset = -(int)(filter.Kernel.GetUpperBound(0) / 2);
            int xMinimum = -xOffset;
            int xMaximum = this.bitmap.Width - filter.Kernel.GetUpperBound(1);
            int yMinimum = -yOffset;
            int yMaximum = this.bitmap.Height - filter.Kernel.GetUpperBound(0);
            int rowMaximum = filter.Kernel.GetUpperBound(0);
            int columnMaximum = filter.Kernel.GetUpperBound(1);

            for (int x = xMinimum; x <= xMaximum; x++)
            {
                for (int y = yMinimum; y <= yMaximum; y++)
                {
                    bool skipPixel = false;

                    float red = 0;
                    float green = 0;
                    float blue = 0;

                    for (int row = 0; row <= rowMaximum; row++)
                    {
                        for (int column = 0; column <= columnMaximum; column++)
                        {
                            int indexX = x + column + xOffset;
                            int indexY = y + row + yOffset;

                            byte newRed;
                            byte newGreen;
                            byte newBlue;
                            byte newAlpha;

                            GetPixel(indexX, indexY, out newRed, out newGreen, out newBlue, out newAlpha);

                            if (newAlpha == 0)
                            {
                                skipPixel = true;

                                break;
                            }

                            red += newRed * filter.Kernel[row, column];
                            green += newGreen * filter.Kernel[row, column];
                            blue += newBlue * filter.Kernel[row, column];
                        }

                        if (skipPixel)
                        {
                            break;
                        }
                    }

                    if (!skipPixel)
                    {
                        red = filter.Offset + red / filter.Weight;

                        if (red < 0)
                        {
                            red = 0;
                        }

                        if (red > 255)
                        {
                            red = 255;
                        }

                        green = filter.Offset + green / filter.Weight;

                        if (green < 0)
                        {
                            green = 0;
                        }

                        if (green > 255)
                        {
                            green = 255;
                        }

                        blue = filter.Offset + blue / filter.Weight;

                        if (blue < 0)
                        {
                            blue = 0;
                        }

                        if (blue > 255)
                        {
                            blue = 255;
                        }

                        helper.SetPixel(x, y, (byte)red, (byte)green, (byte)blue, GetAlpha(x, y));
                    }
                }
            }

            if (!lockResult)
            {
                helper.UnlockBitmap();
            }

            if (!lockStatus)
            {
                UnlockBitmap();
            }

            return helper;
        }

        #endregion

        #region 최대 랭크 적용하기 - ApplyRankMaximum(rank, lockResult)

        /// <summary>
        /// 최대 랭크 적용하기
        /// </summary>
        /// <param name="rank">랭크</param>
        /// <param name="lockResult">잠금 결과</param>
        /// <returns>비트맵 32 헬퍼</returns>
        /// <remarks>영역에서 최대 밝기 값을 사용한다.</remarks>
        public Bitmap32Helper ApplyRankMaximum(int rank, bool lockResult)
        {
            Bitmap32Helper helper = this.Clone();

            bool lockStatus = this.isLocked;

            LockBitmap();

            helper.LockBitmap();

            int startIndex = -rank / 2;
            int stopIndex = rank + startIndex;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int currentBrightness = GetRed(x, y) + GetGreen(x, y) + GetBlue(x, y);
                    int currentRow = y;
                    int currentColumn = x;

                    for (int dy = startIndex; dy < stopIndex; dy++)
                    {
                        int testRow = y + dy;

                        if ((testRow >= 0) && (testRow < Height))
                        {
                            for (int dx = startIndex; dx < stopIndex; dx++)
                            {
                                int testColumn = x + dx;

                                if ((testColumn >= 0) && (testColumn < Width))
                                {
                                    int testBrightness = GetRed(testColumn, testRow) + GetGreen(testColumn, testRow) + GetBlue(testColumn, testRow);

                                    if (testBrightness > currentBrightness)
                                    {
                                        currentBrightness = testBrightness;
                                        currentRow = testRow;
                                        currentColumn = testColumn;
                                    }
                                }
                            }
                        }
                    }

                    helper.SetPixel
                    (
                        x,
                        y,
                        GetRed(currentColumn, currentRow),
                        GetGreen(currentColumn, currentRow),
                        GetBlue(currentColumn, currentRow),
                        255
                    );
                }
            }

            if (!lockResult)
            {
                helper.UnlockBitmap();
            }

            if (!lockStatus)
            {
                UnlockBitmap();
            }

            return helper;
        }

        #endregion
        #region 최소 랭크 적용하기 - ApplyRankMinimum(rank, lockResult)

        /// <summary>
        /// 최소 랭크 적용하기
        /// </summary>
        /// <param name="rank">랭크</param>
        /// <param name="lockResult">잠금 결과</param>
        /// <returns>비트맵 32 헬퍼</returns>
        /// <remarks>영역에서 최소 밝기 값을 사용한다.</remarks>
        public Bitmap32Helper ApplyRankMinimum(int rank, bool lockResult)
        {
            Bitmap32Helper helper = this.Clone();

            bool lockStatus = this.isLocked;

            LockBitmap();

            helper.LockBitmap();

            int startIndex = -rank / 2;
            int stopIndex = rank + startIndex;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int currentBrightness = GetRed(x, y) + GetGreen(x, y) + GetBlue(x, y);
                    int currentRow = y;
                    int currentColumn = x;

                    for (int dy = startIndex; dy < stopIndex; dy++)
                    {
                        int testRow = y + dy;

                        if ((testRow >= 0) && (testRow < Height))
                        {
                            for (int dx = startIndex; dx < stopIndex; dx++)
                            {
                                int testColumn = x + dx;

                                if ((testColumn >= 0) && (testColumn < Width))
                                {
                                    int testBrightness = GetRed(testColumn, testRow) + GetGreen(testColumn, testRow) + GetBlue(testColumn, testRow);

                                    if (testBrightness < currentBrightness)
                                    {
                                        currentBrightness = testBrightness;
                                        currentRow = testRow;
                                        currentColumn = testColumn;
                                    }
                                }
                            }
                        }
                    }

                    helper.SetPixel
                    (
                        x,
                        y,
                        GetRed(currentColumn, currentRow),
                        GetGreen(currentColumn, currentRow),
                        GetBlue(currentColumn, currentRow),
                        255
                    );
                }
            }

            if (!lockResult)
            {
                helper.UnlockBitmap();
            }

            if (!lockStatus)
            {
                UnlockBitmap();
            }

            return helper;
        }

        #endregion
        #region 픽셀화 하기 - Pixellate(rank, lockResult)

        /// <summary>
        /// 픽셀화 하기
        /// </summary>
        /// <param name="rank">랭크</param>
        /// <param name="lockResult">잠금 결과</param>
        public void Pixellate(int rank, bool lockResult)
        {
            bool lockStatus = this.isLocked;

            LockBitmap();

            for (int y = 0; y < Height; y += rank)
            {
                for (int x = 0; x < Width; x += rank)
                {
                    int totalRed = 0;
                    int totalGreen = 0;
                    int totalBlue = 0;
                    int pixelCount = 0;

                    for (int row = y; row < y + rank; row++)
                    {
                        if (row < Height)
                        {
                            for (int column = x; column < x + rank; column++)
                            {
                                if (column < Width)
                                {
                                    totalRed += GetRed(column, row);
                                    totalGreen += GetGreen(column, row);
                                    totalBlue += GetBlue(column, row);

                                    pixelCount++;
                                }
                            }
                        }
                    }

                    byte byteRed = (byte)(totalRed / pixelCount);
                    byte byteGreen = (byte)(totalGreen / pixelCount);
                    byte byteBlue = (byte)(totalBlue / pixelCount);

                    for (int row = y; row < y + rank; row++)
                    {
                        if (row < Height)
                        {
                            for (int column = x; column < x + rank; column++)
                            {
                                if (column < Width)
                                {
                                    SetPixel(column, row, byteRed, byteGreen, byteBlue, 255);
                                }
                            }
                        }
                    }
                }
            }

            if (!lockStatus)
            {
                UnlockBitmap();
            }
        }

        #endregion
        #region 포인트화 하기 - Pointellate(rank, pointDiameter, lockResult)

        /// <summary>
        /// 포인트화 하기
        /// </summary>
        /// <param name="rank">랭크</param>
        /// <param name="pointDiameter">포인트 직경</param>
        /// <param name="lockResult">잠금 결과</param>
        /// <returns>비트맵</returns>
        public Bitmap Pointellate(int rank, int pointDiameter, bool lockResult)
        {
            bool lockStatus = this.isLocked;

            LockBitmap();

            Bitmap bitmap = new Bitmap(Width, Height);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                for (int y = 0; y < Height; y += rank)
                {
                    for (int x = 0; x < Width; x += rank)
                    {
                        int totalRed = 0;
                        int totalGreen = 0;
                        int totalBlue = 0;
                        int pixelCount = 0;

                        for (int row = y; row < y + rank; row++)
                        {
                            if (row < Height)
                            {
                                for (int column = x; column < x + rank; column++)
                                {
                                    if (column < Width)
                                    {
                                        totalRed += GetRed(column, row);
                                        totalGreen += GetGreen(column, row);
                                        totalBlue += GetBlue(column, row);

                                        pixelCount++;
                                    }
                                }
                            }
                        }

                        byte byteRed = (byte)(totalRed / pixelCount);
                        byte byteGreen = (byte)(totalGreen / pixelCount);
                        byte byteBlue = (byte)(totalBlue / pixelCount);

                        int offset = (rank - pointDiameter) / 2;

                        using (Brush brush = new SolidBrush(Color.FromArgb(255, byteRed, byteGreen, byteBlue)))
                        {
                            graphics.FillEllipse
                            (
                                brush,
                                x + offset,
                                y + offset,
                                pointDiameter,
                                pointDiameter
                            );
                        }
                    }
                }
            }

            if (!lockStatus)
            {
                UnlockBitmap();
            }

            return bitmap;
        }

        #endregion

        #region 영역 채우기 - FloodFill(x, y, newColor)

        public List<Point> PointList { get; set; }

        /// <summary>
        /// 영역 채우기
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="newColor">신규 색상</param>
        public void FloodFill(int x, int y, Color newColor)
        {
            if (!this.isLocked)
            {
                throw new InvalidOperationException("영역 채우기 작업 전 잠금이 설정되어야 합니다.");
            }

            byte previousRed;
            byte previousGreen;
            byte previousBlue;
            byte previousAlpha;

            GetPixel(x, y, out previousRed, out previousGreen, out previousBlue, out previousAlpha);

            byte newRed = newColor.R;
            byte newGreen = newColor.G;
            byte newBlue = newColor.B;
            byte newAlpha = newColor.A;

            if ((previousRed == newRed) && (previousGreen == newGreen) && (previousBlue == newBlue) && (previousAlpha == newAlpha))
            {
                return;
            }

            Stack<Point> pointStack = new Stack<Point>();

            pointStack.Push(new Point(x, y));

            SetPixel(x, y, newRed, newGreen, newBlue, newAlpha);
            if (this.PointList != null) this.PointList.Add(new Point(x, y));

            while (pointStack.Count > 0)
            {
                Point point = pointStack.Pop();

                if (point.X > 0)
                {
                    CheckPoint
                    (
                        pointStack,
                        point.X - 1,
                        point.Y,
                        previousRed,
                        previousGreen,
                        previousBlue,
                        previousAlpha,
                        newRed,
                        newGreen,
                        newBlue,
                        newAlpha
                    );
                }

                if (point.Y > 0)
                {
                    CheckPoint
                    (
                        pointStack,
                        point.X,
                        point.Y - 1,
                        previousRed,
                        previousGreen,
                        previousBlue,
                        previousAlpha,
                        newRed,
                        newGreen,
                        newBlue,
                        newAlpha
                    );
                }

                if (point.X < Width - 1)
                {
                    CheckPoint
                    (
                        pointStack,
                        point.X + 1,
                        point.Y,
                        previousRed,
                        previousGreen,
                        previousBlue,
                        previousAlpha,
                        newRed,
                        newGreen,
                        newBlue,
                        newAlpha
                    );
                }

                if (point.Y < Height - 1)
                {
                    CheckPoint
                    (
                        pointStack,
                        point.X,
                        point.Y + 1,
                        previousRed,
                        previousGreen,
                        previousBlue,
                        previousAlpha,
                        newRed,
                        newGreen,
                        newBlue,
                        newAlpha
                    );
                }
            }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////// Private

        #region 포인트 체크하기 - CheckPoint(pointStack, x, y, previousRed, previousGreen, previousBlue, previousAlpha, newRed, newGreen, newBlue, newAlpha)

        /// <summary>
        /// 포인트 체크하기
        /// </summary>
        /// <param name="pointStack">포인트 스택</param>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="previousRed">이전 빨강색 채널</param>
        /// <param name="previousGreen">이전 녹색 채널</param>
        /// <param name="previousBlue">이전 파랑색 채널</param>
        /// <param name="previousAlpha">이전 투명도 채널</param>
        /// <param name="newRed">신규 빨강색 채널</param>
        /// <param name="newGreen">신규 녹색 채널</param>
        /// <param name="newBlue">신규 파랑색 채널</param>
        /// <param name="newAlpha">신규 투명도 채널</param>
        private void CheckPoint(Stack<Point> pointStack, int x, int y, byte previousRed, byte previousGreen, byte previousBlue, byte previousAlpha, byte newRed, byte newGreen, byte newBlue, byte newAlpha)
        {
            byte red;
            byte green;
            byte blue;
            byte alpha;

            GetPixel(x, y, out red, out green, out blue, out alpha);

            if ((red == previousRed) && (green == previousGreen) && (blue == previousBlue) && (alpha == previousAlpha))
            {
                pointStack.Push(new Point(x, y));

                SetPixel(x, y, newRed, newGreen, newBlue, newAlpha);
                if (this.PointList != null) this.PointList.Add(new Point(x, y));
            }
        }

        #endregion
    }
}