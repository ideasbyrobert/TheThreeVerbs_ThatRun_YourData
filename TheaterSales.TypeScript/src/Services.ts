import {Theater, TheaterSalesAggregate} from './Domain'
import {ITopPerformingTheatersRepository} from './Repository'

export interface ITopPerformingTheatersService
{
  getHighestSalesTheater(date: string): Theater | null
  getTopPerformingTheaters(
    startDate: string, endDate: string, topCount: number): TheaterSalesAggregate[]
  getTheaterSalesByDate(date: string): TheaterSalesAggregate[]
  getUnderperformingTheaters(
    date: string, threshold?: number): TheaterSalesAggregate[]
}

export class TopPerformingTheatersService implements ITopPerformingTheatersService
{
  constructor(private readonly repository: ITopPerformingTheatersRepository) { }

  getHighestSalesTheater = (date: string): Theater | null =>
    this.repository.getTheaterSalesByDate(date)
      .reduce(higherSalesAggregate, null as TheaterSalesAggregate | null)
      ?.theater ?? null;

  getTopPerformingTheaters = (
    startDate: string, endDate: string, topCount: number): TheaterSalesAggregate[] =>
    this.repository.getTopPerformingTheaters(startDate, endDate, topCount)

  getTheaterSalesByDate = (date: string): TheaterSalesAggregate[] =>
    this.repository.getTheaterSalesByDate(date)

  getUnderperformingTheaters = (
    date: string, threshold: number = 0): TheaterSalesAggregate[] =>
    this.repository.getUnderperformingTheaters(date, threshold)
}

const higherSalesAggregate = (
  current: TheaterSalesAggregate | null,
  next: TheaterSalesAggregate): TheaterSalesAggregate | null =>
  current === null || next.totalSales > current.totalSales ? next : current