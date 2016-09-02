/// <binding AfterBuild='default' />
/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp');
var less = require('gulp-less');
var cssnano = require('gulp-cssnano');
var rename = require('gulp-rename');
var bower = require('gulp-bower');

var paths = {
    less:   './wwwroot/css/*.less',
    css:    './wwwroot/css'
};

gulp.task('default', ['less'], function () {
    // place code for your default task here
});

gulp.task('bower', function (cb) {
    return bower(cb);
});

gulp.task('less', ['bower'], function () {
    return gulp.src(paths.less)
        .pipe(less())
        .pipe(gulp.dest(paths.css))
        .pipe(cssnano())
        .pipe(rename({
            suffix: '.min'
        }))
        .pipe(gulp.dest(paths.css));
});
