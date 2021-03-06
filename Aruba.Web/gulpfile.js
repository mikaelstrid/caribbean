/// <binding />
var mainBowerFiles = require('main-bower-files');
var gulp = require('gulp');
var gulpFilter = require('gulp-filter');
var gulpRubySass = require('gulp-ruby-sass');

gulp.task('default', ['watch-sass']);

gulp.task('bower-js', function () {
    gulp.src(mainBowerFiles())
        .pipe(gulpFilter('*.js'))
        .pipe(gulp.dest('./Scripts/lib'));
});

gulp.task('bower-css', function () {
    gulp.src(mainBowerFiles())
        .pipe(gulpFilter('*.css'))
		.pipe(gulp.dest('./Stylesheets/lib'));
});

gulp.task('sass', function () {
    return gulpRubySass('./Stylesheets/scss/**/*.scss', {
        loadPath: ['bower_components/foundation/scss']
    })
      .on('error', gulpRubySass.logError)
      .pipe(gulp.dest('./Stylesheets'));
});

gulp.task('watch-sass', function () {
    gulp.watch('./Stylesheets/**/*.scss', ['sass']);
});
