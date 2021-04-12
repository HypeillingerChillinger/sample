    public class MyModel
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
    }

    public class RankData<MyModel>
    {
        public long Rank { get; set; }
        public MyModel Model { get; set; }
    }

    public class MultipleConcat
    {
        public void Start()
        {
            var start = new DateTime(2000, 1, 1);
            var end = new DateTime(2000, 1, 2);



            var context = new DataConnection(new SQLiteDataProvider(ProviderName.SQLiteClassic), "Data Source=:memory:");
            context.CreateTable<Car>();


            var fluentMappingBuilder = context.MappingSchema.GetFluentMappingBuilder();

            var carBuilder = fluentMappingBuilder.Entity<Car>();
            carBuilder.Property(x => x.Id).IsPrimaryKey();





            var carTable = context.GetTable<Car>();

            var main = (from car in carTable
                        select new RankData<MyModel>
                        {
                            Model = { Id = car.Id, Date = car.DateTime },
                            Rank = Sql.Ext.RowNumber().Over().PartitionBy(car.Id).OrderBy(car.Id).ToValue()
                        }).Where(x => x.Rank == 1).Select(x => x.Model).AsSubQuery();

            var first = main.Where(x => start <= x.Date && x.Date <= end);

            var second = main.Where(x => x.Date < start).OrderByDescending(x => x.Date).Take(1);
            var third = main.Where(x => end < x.Date).OrderBy(x => x.Date).Take(1);

            var res = first.Concat(second).Concat(third).ToList();



            var lastQuery = context.LastQuery;

            File.WriteAllText("bla11212.txt", lastQuery);
        }
    }
