namespace YLWService
{
    /// <summary>
    /// 필터
    /// </summary>
    public class Filter
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////// Field
        ////////////////////////////////////////////////////////////////////////////////////////// Public

        #region Field

        /// <summary>
        /// 커널
        /// </summary>
        public float[,] Kernel;

        /// <summary>
        /// 가중치
        /// </summary>
        public float Weight;

        /// <summary>
        /// 오프셋
        /// </summary>
        public float Offset;

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////// Method
        ////////////////////////////////////////////////////////////////////////////////////////// Public

        #region 정규화 하기 - Normalize()

        /// <summary>
        /// 정규화 하기
        /// </summary>
        public void Normalize()
        {
            Weight = 0;

            for (int y = 0; y <= Kernel.GetUpperBound(0); y++)
            {
                for (int x = 0; x <= Kernel.GetUpperBound(1); x++)
                {
                    Weight += Kernel[y, x];
                }
            }
        }

        #endregion
        #region 제로 커널 설정하기 - SetZeroKernel()

        /// <summary>
        /// 제로 커널 설정하기
        /// </summary>
        /// <remarks>커널의 합이 제로가 되도록 중앙 커널 계수의 값을 설정한다.</remarks>
        public void SetZeroKernel()
        {
            float total = 0;

            for (int y = 0; y <= Kernel.GetUpperBound(0); y++)
            {
                for (int x = 0; x <= Kernel.GetUpperBound(1); x++)
                {
                    total += Kernel[y, x];
                }
            }

            int rowMiddle = (int)(Kernel.GetUpperBound(0) / 2);
            int columnMiddle = (int)(Kernel.GetUpperBound(1) / 2);

            total -= Kernel[rowMiddle, columnMiddle];

            Kernel[rowMiddle, columnMiddle] = -total;
        }

        #endregion
    }
}