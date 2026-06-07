const test = require('node:test');
const assert = require('node:assert/strict');

const { computeClickDelayMs } = require('../dist/clicker.js');

test('computeClickDelayMs converts CPS to a safe interval', () => {
  assert.equal(computeClickDelayMs(10), 100);
  assert.equal(computeClickDelayMs(20), 50);
  assert.equal(computeClickDelayMs(0), 1000);
});
