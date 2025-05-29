export interface Theater
{
  readonly id: number
  readonly name: string
}

export interface Movie
{
  readonly id: number
  readonly title: string
}

export interface Sale
{
  readonly id: number
  readonly theaterId: number
  readonly movieId: number
  readonly saleDate: string
  readonly amount: number
}

export interface TheaterSalesAggregate
{
  readonly theater: Theater
  readonly totalSales: number
}