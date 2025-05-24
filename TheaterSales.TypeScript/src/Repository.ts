import {TheaterSalesContext} from './Data'
import {TheaterSalesAggregate, Theater, Sale} from './Domain'

export interface ITopPerformingTheatersRepository
{
  getTheaterSalesByDate(date: string): TheaterSalesAggregate[]
  getTopPerformingTheaters(
    startDate: string, endDate: string, topCount: number): TheaterSalesAggregate[]
  getUnderperformingTheaters(
    date: string, threshold?: number): TheaterSalesAggregate[]
}

export class SalesRepository implements ITopPerformingTheatersRepository
{
  constructor(private readonly context: TheaterSalesContext) { }

  private get theaters(): Theater[] {return [...this.context.Theaters]}
  private get sales(): Sale[] {return [...this.context.Sales]}

  private salesOn = (date: string): Sale[] =>
    this.sales.filter(sale => sale.saleDate === date)

  private salesBetween = (startDate: string, endDate: string): Sale[] =>
    this.sales.filter(sale => sale.saleDate >= startDate && sale.saleDate <= endDate)

  private salesForTheater = (theaterId: number, date: string): Sale[] =>
    this.salesOn(date).filter(sale => sale.theaterId === theaterId)

  private totalRevenue = (sales: Sale[]): number =>
    sales.reduce((sum, sale) => sum + sale.amount, 0)

  private aggregateTheaterSales = (theater: Theater, date: string): TheaterSalesAggregate =>
    ({theater, totalSales: this.totalRevenue(this.salesForTheater(theater.id, date))})

  private aggregateTheaterSalesPeriod = (
    theater: Theater, startDate: string, endDate: string): TheaterSalesAggregate =>
  ({
    theater, totalSales: this.totalRevenue(
      this.salesBetween(startDate, endDate).filter(s => s.theaterId === theater.id))
  })

  getTheaterSalesByDate = (date: string): TheaterSalesAggregate[] =>
    this.theaters
      .map(theater => this.aggregateTheaterSales(theater, date))
      .sort((a, b) => b.totalSales - a.totalSales)

  getTopPerformingTheaters = (
    startDate: string, endDate: string, topCount: number): TheaterSalesAggregate[] =>
    this.theaters
      .map(theater => this.aggregateTheaterSalesPeriod(theater, startDate, endDate))
      .sort((a, b) => b.totalSales - a.totalSales)
      .reduce<[TheaterSalesAggregate[], number]>((acc, item) =>
        acc[1] < topCount
          ? [[...acc[0], item], acc[1] + 1]
          : acc, [[], 0])[0];

  getUnderperformingTheaters = (
    date: string, threshold: number = 0): TheaterSalesAggregate[] =>
    this.getTheaterSalesByDate(date).filter(aggregate => aggregate.totalSales <= threshold)
}