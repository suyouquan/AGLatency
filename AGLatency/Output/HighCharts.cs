using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Reflection;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;



namespace AGLatency.Output
{
    public static class HighCharts
    {

        public static int divId = 0;

        private static string jsContent =
          @"
<div id='container' style='min-width:300px;max-width:900px; height:{HEIGHT}px; margin: 0 auto;margin-left:0px;'></div>

   <script>
/*
https://api.highcharts.com/highcharts/yAxis.dateTimeLabelFormats
{
    millisecond: '%H:%M:%S.%L',
    second: '%H:%M:%S',
    minute: '%H:%M',
    hour: '%H:%M',
    day: '%e. %b',
    week: '%e. %b',
    month: '%b \'%y',
    year: '%Y'
}
*/

Highcharts.chart('container', {
  chart: {
    type: 'line',
     zoomType: 'x' 
   
            
  },
  title: {
    text: '{0}'
  },
  subtitle: {
    text: '{1}'
  },
  xAxis: {
    type: 'datetime',
    dateTimeLabelFormats: { // don't display the dummy year
      millisecond:'%H:%M:%S.%L',
	  month: '%e. %b',
      year: '%b'
    },
    title: {
      text: '{2}'
    }
  },
  yAxis: {
    title: {
      text: '{3}'
    },
    min: 0
  },
  tooltip: {
    headerFormat: '<b>{series.name}</b><br>',
    pointFormat: '{point.x:%e. %b  %H:%M:%S.%L}: {point.y:.0f}'
  },

  plotOptions: {
    spline: {
      marker: {
        enabled: true,
        radius: 3
       
      }
    }
  },
 
  colors: ['{COLOR}','#357EC7', 'green', 'cyan', 'pink','gold'], 

  // Define the data points. All series have a dummy year
  // of 1970/71 in order to be compared on the same x axis. Note
  // that in JavaScript, months start at 0 for January, 1 for February etc.
  series: [{4}]
});   
   </script>";
        /*
          {
    name: "Primary - Log Block Local Flush",
    data: [
      [Date.UTC(1970, 10, 25,10,20,55), 1],
      [Date.UTC(1970, 10, 25,10,20,56), 2],
	  [Date.UTC(1970, 10, 25,10,20,57), 3],
      
	  [Date.UTC(1970, 10, 25,10,20,58), 1],
      [Date.UTC(1970, 10, 25,10,20,59), 2],
	  [Date.UTC(1970, 10, 25,10,21,0), 3],
      
	  [Date.UTC(1970, 10, 25,10,21,22), 2],
      [Date.UTC(1970, 10, 25,10,21,35), 1],
	  [Date.UTC(1970, 10, 25,10,21,55), 5],
      
	  
    ]
  },
         */
        private static string GetSeriesJSON(string title, List<string> lst)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("name:\"" + title + "\",");
            sb.AppendLine("data:[");
            foreach (string s in lst)
            {
                sb.AppendLine("[" + s + "],");
            }
            sb.AppendLine("]");
            sb.AppendLine("},");

            return sb.ToString();
        }

        public static string GetChartHtml(string Title, string subTitle, string xText, string yText, Dictionary<string, List<string>> dict, string color,int height=300)
        {
            string chartData = "";
            foreach (string s in dict.Keys)
            {
                chartData += GetSeriesJSON(s, dict[s]);
            }
            string div = "container" + divId;
            divId++;
            string result = jsContent.Replace("{0}", Title).Replace("{1}", subTitle)
                .Replace("{2}", xText).Replace("{3}", yText)
                .Replace("{4}", chartData).Replace
                ("container", div).Replace("{COLOR}",color).Replace("{HEIGHT}",height.ToString());

            return HttpUtility.JavaScriptStringEncode(result);

        }
        public static string BarJS(string divName)
        {
            string bar = @"
<div id='" + divName + @"' style='min-width:300px;max-width:900px; height:{HEIGHT}px; margin: 0 auto;margin-left:0px;'></div>

   <script>
var chart=Highcharts.chart('" + divName + @"', {
    chart: {
        type: 'column'
    },
    title: {
        text: '{TITLE}'
    },
    subtitle: {
        text: '{SUBTITLE}'
    },
    xAxis: {
        categories: [{XAXIS}],
        title: {
            text: null
        }
    },
    yAxis: {
        min: 0,
        title: {
            text: '{YTEXT}',
            align: 'high'
        },
        labels: {
            overflow: 'justify'
        }
    },
    tooltip: {
        valueSuffix: ' microseconds'
    },
    plotOptions: {
    series: {
			borderWidth: 0,
			dataLabels: {
				enabled: true,
				format: '{point.y:.0f}'
			}
		},

        bar: {
            dataLabels: {
                enabled: true,
               formatter: function () {
                         return this.y+'';
                  }
    }
        }
    },
    credits: {
        enabled: false
    },

legend: {
        layout: 'vertical',
        align: 'right',
        verticalAlign: 'top',
        x: -40,
        y: 80,
        floating: true,
        borderWidth: 1,
        backgroundColor: ((Highcharts.theme && Highcharts.theme.legendBackgroundColor) || '#FFFFFF'),
        shadow: true
    },

 series: [{
        name: '',
         showInLegend: false,      
         pointWidth: 60,
        data: [{DATA}]
    }]

    });

 
var max = 7;
 var colors= ['#ff9999', '#ffcc00', ' #66b3ff','#00cc7a','#CC99FF','#339999','green']
var len=chart.series[0].data.length;
$.each(chart.series[0].data, function(i,data){

   for (var n = 0; n <= len; n++) {
      var k=n%max;
	  if(k>=max)k=max-1;
      chart.series[0].data[n].update({color:colors[k]});

   }
});  

</script>
";
            return bar;
        }
        public static string GetBarHtml(string name, string title, string subTitle, string xText, string yText, Dictionary<string, int> data)
        {
            StringBuilder sb = new StringBuilder();
            string categories = "";
         
            foreach (KeyValuePair<string, int> kv in data)
            {
                sb.Append(kv.Value.ToString()+",");

                 
                categories = categories + "'" + kv.Key + "',";

            }
            var height = data.Count * 40+100;
            string barJs = BarJS(name);

            string result = barJs.Replace("{SUBTITLE}", subTitle).Replace("{TITLE}", title).Replace("{YTEXT}", yText)
            .Replace("{XAXIS}", categories).Replace("{DATA}", sb.ToString()).Replace("{HEIGHT}",height.ToString());

           //  return result;
            return HttpUtility.JavaScriptStringEncode(result);
        }




    }
}
