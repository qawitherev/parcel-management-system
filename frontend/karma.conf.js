module.exports = function (config) {
  config.set({
    frameworks: ['jasmine', '@angular-devkit/build-angular'],
    browsers: ['ChromeHeadlessCI'],
    reporters: ['progress', 'junit'], 
    junitReporter: {
      outputDir: 'test-results', // folder where to save files
      outputFile: 'unit-test-results-frontend.xml', // name of the report file
      useBrowserName: false // don't append browser name to the file
    },
    customLaunchers: {
      ChromeHeadlessCI: {
        base: 'ChromeHeadless',
        flags: [
          '--no-sandbox',
          '--disable-gpu',
          '--disable-dev-shm-usage',
          '--remote-debugging-port=9222'
        ]
      }
    },
    plugins: [
      require('karma-jasmine'),
      require('karma-chrome-launcher'),
      require('karma-jasmine-html-reporter'),
      require('karma-coverage'),
      require('karma-junit-reporter')
    //   require('@angular-devkit/build-angular/plugins/karma')
    ],
    port: 9876,
    colors: true,
    logLevel: config.LOG_INFO,
    autoWatch: false,
    singleRun: true,
  });
};
