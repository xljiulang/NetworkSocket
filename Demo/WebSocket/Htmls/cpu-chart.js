// cpuÏßÍ¼
var cpu = new function () {
    var myLine;

    var lineChartData = {
        labels: [],
        datasets: [
            {
                label: "My Second dataset",
                fillColor: "rgba(151,187,205,0.2)",
                strokeColor: "rgba(151,187,205,1)",
                pointColor: "rgba(151,187,205,1)",
                pointStrokeColor: "#fff",
                pointHighlightFill: "#fff",
                pointHighlightStroke: "rgba(151,187,205,1)",
                data: []
            }
        ]
    }

    window.onload = function () {
        var ctx = document.getElementById("canvas").getContext("2d");
        myLine = new Chart(ctx).Line(lineChartData, { responsive: true });
    };

    // »æÖÆÏßÍ¼
    this.draw = function (data) {
        if (myLine.datasets[0].points.length > 15) {
            myLine.removeData();
        }
        myLine.addData([data.value], data.time);
    }
};