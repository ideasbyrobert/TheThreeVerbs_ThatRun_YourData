namespace FunctionalSQL.Server.Domain;

public record Theater(int Id, string Name);

public record Movie(int Id, string Title);

public record Sale(
    int Id,
    int TheaterId,
    int MovieId,
    DateOnly SaleDate,
    decimal Amount
);

public record TheaterSalesAggregate(
    Theater Theater,
    decimal TotalSales
);