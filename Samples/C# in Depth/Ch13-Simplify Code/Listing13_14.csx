#r ".\Chapter13.exe"
#r "WindowsBase"

using Chapter13;
using System.Windows;

Func<Square> squareFactory = () => new Square(new Point(5, 5), 10);
Func<IShape> shapeFactory = squareFactory;

Action<IShape> shapePrinter = shape => Console.WriteLine(shape.Area);
Action<Square> squarePrinter = shapePrinter;

squarePrinter(squareFactory());
shapePrinter(shapeFactory());