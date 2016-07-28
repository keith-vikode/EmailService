/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp');
var less = require('gulp-less');

gulp.task('default', ['less'], function () {
    // place code for your default task here
});

gulp.task('less', function () {
    return gulp.src('./wwwroot/css/*.less')
      .pipe(less())
      .pipe(gulp.dest('./wwwroot/css'));
});
