var mainBowerFiles = require('main-bower-files');
var gulp = require('gulp'),
    gulpFilter = require('gulp-filter');

gulp.task('default', function () {
    console.log("Hello world!");
    // place code for your default task here
});

gulp.task('js', function () {
    gulp.src(mainBowerFiles())
        .pipe(gulpFilter('*.js'))
        .pipe(gulp.dest('./Scripts/lib'));
});

gulp.task('css', function () {
    gulp.src(mainBowerFiles())
        .pipe(gulpFilter('*.css'))
		.pipe(gulp.dest('./Content/lib'));
});