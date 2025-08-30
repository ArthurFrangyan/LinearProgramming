using SimplexMethod;

Console.WriteLine("Write Your Table, Press Enter to Exit Input Mode.");
var table = new Table(Console.ReadLine);

var result = Simplex.Calculate(table);

Console.WriteLine(result);