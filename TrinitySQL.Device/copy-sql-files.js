const fs = require('fs')
const path = require('path')

const sqlDir = path.join(__dirname, '..', 'TrinitySQL.Context')
const distDir = path.join(__dirname, 'dist')

if (!fs.existsSync(distDir))
{
  fs.mkdirSync(distDir, {recursive: true})
}

const sqlFiles = ['schema.sql', 'test_fixtures.sql']
sqlFiles.forEach(file =>
{
  const source = path.join(sqlDir, file)
  const dest = path.join(distDir, file)
  if (fs.existsSync(source))
  {
    fs.copyFileSync(source, dest)
    console.log(`Copied ${file} to dist/`)
  }
})