const path = require('path');

module.exports = {
  testEnvironment: 'node',
  rootDir: path.resolve(__dirname, '../..'),
  roots: ['<rootDir>/TrinitySQL.Blueprint/Device'],
  testMatch: ['**/*.test.ts'],
  moduleNameMapper: {
    '^@device/(.*)$': '<rootDir>/TrinitySQL.Device/src/$1'
  },
  collectCoverageFrom: [
    '<rootDir>/TrinitySQL.Device/src/**/*.ts'
  ],
  transform: {
    '^.+\\.ts$': [require.resolve('ts-jest'), {
      tsconfig: '<rootDir>/TrinitySQL.Blueprint/Device/tsconfig.json'
    }]
  },
  moduleFileExtensions: ['ts', 'js', 'json', 'node']
};