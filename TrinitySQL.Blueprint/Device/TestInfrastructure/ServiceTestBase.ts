import {TheaterSalesContext} from '@device/Data'
import {TestDatabaseBuilder} from './TestDatabaseBuilder'
import {TestDatabase} from './TestDatabase'

export abstract class ServiceTestBase
{
  protected context: TheaterSalesContext = null!
  private testDatabase: TestDatabase = null!
  
  setupTest(): void
  {
    this.testDatabase = new TestDatabaseBuilder().build()
    this.context = this.testDatabase.Context
    this.initializeTest()
  }
  
  teardownTest(): void
  {
    this.cleanupTest()
    this.testDatabase?.dispose()
  }
  
  protected abstract initializeTest(): void
  protected cleanupTest(): void {}
}