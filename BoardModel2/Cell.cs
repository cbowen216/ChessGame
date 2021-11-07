
namespace BoardModel2
{
    public class Cell
    {
        // the properties of a cell
        public int RowNumber { get; set; }
        public int ColumnNumber { get; set; }
        public CellOccupiedBy Occupied { get; set; }
        public string Peice { get; set; }
        public bool LegalNextMove { get; set; }
        public bool Attack { get; set; }
        public bool Selected { get; set; }
        public bool HasKingInCheck { get; set; }

        public Cell(int x, int y)
        {
            RowNumber = x;
            ColumnNumber = y;
        }
    }
}
