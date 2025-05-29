import {SalesRepository} from '@device/Repository'
import {TopPerformingTheatersService} from '@device/Services'
import {ServiceTestBase, TestDates} from '../TestInfrastructure'

describe('TopPerformingTheatersTests', () =>
{
  const testSetup = new class extends ServiceTestBase
  {
    service: TopPerformingTheatersService = null!
    
    protected initializeTest(): void
    {
      const repository = new SalesRepository(this.context)
      this.service = new TopPerformingTheatersService(repository)
    }
  }()
  
  beforeEach(() => testSetup.setupTest())
  afterEach(() => testSetup.teardownTest())

  test('GetHighestSalesTheater_OnPeakSummerDay_ReturnsMultiplex', () =>
  {
    const result = testSetup.service.getHighestSalesTheater(TestDates.IndependenceDay2024)

    expect(result).not.toBeNull()
    expect(result!.name).toBe('Multiplex 20')

    const aggregates = testSetup.service.getTheaterSalesByDate(TestDates.IndependenceDay2024)
    const multiplex = aggregates.find(a => a.theater.name === 'Multiplex 20')
    expect(multiplex!.totalSales).toBe(22900)
  })

  test('GetHighestSalesTheater_OnHolidaySeason_ReturnsMultiplex', () =>
  {
    const result = testSetup.service.getHighestSalesTheater(TestDates.Christmas2024)

    expect(result).not.toBeNull()
    expect(result!.name).toBe('Multiplex 20')

    const aggregates = testSetup.service.getTheaterSalesByDate(TestDates.Christmas2024)
    const multiplex = aggregates.find(a => a.theater.name === 'Multiplex 20')
    expect(multiplex!.totalSales).toBe(12800)
  })

  test('GetHighestSalesTheater_OnRegularDay_ReturnsExpectedTheater', () =>
  {
    const result = testSetup.service.getHighestSalesTheater(TestDates.RegularSpringDay2024)

    expect(result).not.toBeNull()
    expect(result!.name).toBe('Multiplex 20')

    const aggregates = testSetup.service.getTheaterSalesByDate(TestDates.RegularSpringDay2024)
    const multiplex = aggregates.find(a => a.theater.name === 'Multiplex 20')
    expect(multiplex!.totalSales).toBe(18100)
  })

  test('GetTopPerformingTheaters_ReturnsTheatersInDescendingOrder', () =>
  {
    const results = testSetup.service.getTheaterSalesByDate(TestDates.SummerDay2024)

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
    const underperformingTheaters = testSetup.service.getUnderperformingTheaters(TestDates.ZeroSalesDay, 0)

    expect(underperformingTheaters).toHaveLength(6)
    expect(underperformingTheaters.some(t => t.theater.name === 'Drive-In Classic')).toBe(true)
  })
})