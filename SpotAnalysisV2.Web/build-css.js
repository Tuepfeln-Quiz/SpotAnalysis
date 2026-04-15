#!/usr/bin/env node
const fs = require('fs');
const path = require('path');
const postcss = require('postcss');
const tailwindcss = require('tailwindcss');
const autoprefixer = require('autoprefixer');

const inputFile = './wwwroot/css/app.tailwind.css';
const outputFile = './wwwroot/css/app.css';

const css = fs.readFileSync(inputFile, 'utf8');

postcss([tailwindcss, autoprefixer])
  .process(css, { from: inputFile, to: outputFile })
  .then(result => {
    fs.writeFileSync(outputFile, result.css);
    console.log(`✓ Generated ${outputFile}`);
  })
  .catch(error => {
    console.error('Error processing CSS:', error);
    process.exit(1);
  });
