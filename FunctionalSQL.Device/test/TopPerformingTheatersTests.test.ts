import {DatabaseInitializer, TheaterSalesContext} from '../src/Data'
import {SalesRepository} from '../src/Repository'
import {TopPerformingTheatersService} from '../src/Services'
import * as fs from 'fs'
import * as path from 'path'
import {randomUUID} from 'crypto'

describe('TopPerformingTheatersTests', () =>
{
  let dbPath: string
  let connectionString: string
  let initializer: DatabaseInitializer
  let context: TheaterSalesContext
  let service: TopPerformingTheatersService

  beforeEach(() =>
  {
    dbPath = path.join(require('os').tmpdir(), `theater_sales_test_${randomUUID()}.db`)
    connectionString = dbPath

    initializer = new DatabaseInitializer(connectionString)
    initializer.Initialize()
    initializer.SeedData()

    context = new TheaterSalesContext(connectionString)
    const repository = new SalesRepository(context)
    service = new TopPerformingTheatersService(repository)
  })

  afterEach(() =>
  {
    context.close()
    if (fs.existsSync(dbPath))
    {
      fs.unlinkSync(dbPath)
    }
  })

  test('GetHighestSalesTheater_OnPeakSummerDay_ReturnsMultiplex', () =>
  {
    const peakSummerDay = '2024-07-04'
    const result = service.getHighestSalesTheater(peakSummerDay)

    expect(result).not.toBeNull()
    expect(result!.name).toBe('Multiplex 20')

    const aggregates = service.getTheaterSalesByDate(peakSummerDay)
    const multiplex = aggregates.find(a => a.theater.name === 'Multiplex 20')
    expect(multiplex!.totalSales).toBe(22900)
  })

  test('GetHighestSalesTheater_OnHolidaySeason_ReturnsMultiplex', () =>
  {
    const christmasDay = '2024-12-25'
    const result = service.getHighestSalesTheater(christmasDay)

    expect(result).not.toBeNull()
    expect(result!.name).toBe('Multiplex 20')

    const aggregates = service.getTheaterSalesByDate(christmasDay)
    const multiplex = aggregates.find(a => a.theater.name === 'Multiplex 20')
    expect(multiplex!.totalSales).toBe(12800)
  })

  test('GetHighestSalesTheater_OnRegularDay_ReturnsExpectedTheater', () =>
  {
    const regularDay = '2024-03-15'
    const result = service.getHighestSalesTheater(regularDay)

    expect(result).not.toBeNull()
    expect(result!.name).toBe('Multiplex 20')

    const aggregates = service.getTheaterSalesByDate(regularDay)
    const multiplex = aggregates.find(a => a.theater.name === 'Multiplex 20')
    expect(multiplex!.totalSales).toBe(18100)
  })

  test('GetTopPerformingTheaters_ReturnsTheatersInDescendingOrder', () =>
  {
    const date = '2024-06-15'
    const results = service.getTheaterSalesByDate(date)

    expect(results.length).toBeGreaterThan(0)

    for (let i = 1; i < results.length; i++)
    {
      expect(results[i - 1].totalSales).toBeGreaterThanOrEqual(results[i].totalSales)
    }

    expect(results[0].theater.name).toBe('IMAX Theater')
    expect(results[0].totalSales).toBe(19300)

    const multiplex = results.find(r => r.theater.name === 'Multiplex 20')
    expect(multiplex).not.toBeNull()
    expect(multiplex!.totalSales).toBe(0)
  })

  test('IdentifyUnderperformingTheaters_FindsTheatersWithZeroSales', () =>
  {
    const date = '2024-05-09'
    const underperformingTheaters = service.getUnderperformingTheaters(date, 0)

    expect(underperformingTheaters).toHaveLength(6)
    expect(underperformingTheaters.some(t => t.theater.name === 'Drive-In Classic')).toBe(true)
  })
})