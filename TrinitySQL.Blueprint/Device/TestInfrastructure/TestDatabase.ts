import {TheaterSalesContext} from '@device/Data'
import * as fs from 'fs'

export class TestDatabase
{
  constructor(
    private readonly context: TheaterSalesContext,
    private readonly dbPath: string
  ) {}
  
  get Context(): TheaterSalesContext
  {
    return this.context
  }
  
  dispose(): void
  {
    this.context.close()
    this.deleteDatabase()
  }
  
  private deleteDatabase(): void
  {
    if (fs.existsSync(this.dbPath))
    {
      fs.unlinkSync(this.dbPath)
    }
  }
}