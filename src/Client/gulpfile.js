/// <binding AfterBuild="default" />

var gulp = require("gulp"),
    merge = require("merge-stream"),
    cssmin = require("gulp-cssmin"),
    rename = require("gulp-rename");

var nodeRoot = "./node_modules/";
var targetPath = "./wwwroot/lib/";

gulp.task("copies", function () {
    var streams = [
        gulp.src(nodeRoot + "bootstrap/dist/**/*").pipe(gulp.dest(targetPath + "/bootstrap/dist"))
    ];
    return merge(streams);
});

gulp.task("css", function () {
    return gulp.src("./wwwroot/src/*.css")
        .pipe(cssmin())
        .pipe(rename({ suffix: '.min' }))
        .pipe(gulp.dest(targetPath + "/phrazy/dist"));
});

gulp.task("default", gulp.series(["copies", "css"]));