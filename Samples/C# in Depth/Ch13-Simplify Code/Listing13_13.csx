#r ".\Chapter13.exe"
#r "WindowsBase"

using Chapter13;
using System.Windows;

List<Circle> circles = new List<Circle> {
    new Circle(new Point(0, 0), 15),
    new Circle(new Point(10, 5), 20),
};
IComparer<IShape> areaComparer = new AreaComparer();
circles.Sort(areaComparer);

class AreaComparer : IComparer<IShape>
{
    public int Compare(IShape x, IShape y)
    {
        return x.Area.CompareTo(y.Area);
    }
}