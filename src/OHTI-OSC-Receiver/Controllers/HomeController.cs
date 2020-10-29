using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace OHTI_OSC_Receiver.Controllers
{
    public class HomeController: Controller
    {
        [HttpGet]
        [Route("~/", Name = "default")]
        public ActionResult Index(string level = "")
        {
            var data = @"
                <!DOCTYPE html>

                <html lang='en' xmlns='http://www.w3.org/1999/xhtml'>
                <head>
                    <meta charset='utf-8' />
                    <title>Larkspur</title>
                    <style type='text/css'>
                        body {
                            font-family: arial, sans-serif;
                            font-size: 10px;
                            background: #0089ca;
                            color: #e7f5ff;
                        }

                        h1 {
                            font-size: 4em;
                            letter-spacing: -3px;
                            text-align: center;
                        }

                        h2 {
                            font-size: 2em;
                        }

                        .row {
                            display: flex;
                            flex-direction: row;
                        }

                        .data-point {
                            flex: 1 1 25%;
                            text-align: center;
                        }

                        .data-point-text {
                            font-size: 2rem;
                            font-weight: 600;
                        }
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <h1>Headtracking <span id='socketStatus'></span></h1>

                        <div class='row'>
                            <div class='data-point'>
                                <div class='data-point-text'>W</div>
                                <div class='data-point-value' id='data-w'></div>
                            </div>
                            <div class='data-point'>
                                <div class='data-point-text'>X</div>
                                <div class='data-point-value' id='data-x'></div>
                            </div>
                            <div class='data-point'>
                                <div class='data-point-text'>Y</div>
                                <div class='data-point-value' id='data-y'></div>
                            </div>
                            <div class='data-point'>
                                <div class='data-point-text'>Z</div>
                                <div class='data-point-value' id='data-z'></div>
                            </div>
                        </div>

                    </div>

                    <script src='https://cdn.jsdelivr.net/npm/@microsoft/signalr@3.1.3/dist/browser/signalr.min.js'></script>

                    <script type='text/javascript'>
                    'use strict';

                    var connection = new signalR.HubConnectionBuilder()
                        .withUrl('/apiHub')
                        .withAutomaticReconnect()
                        .build();

                        setSocketConnectionFeedback('Disconnected');

                    var $dataW = document.getElementById('data-w');
                    var $dataX = document.getElementById('data-x');
                    var $dataY = document.getElementById('data-y');
                    var $dataZ = document.getElementById('data-z');

                    connection.on('HeadtrackerEvent', (path, dataW, dataX, dataY, dataZ) => {
                        //console.log(path, dataW, dataX, dataY, dataZ);
                        requestAnimationFrame(() => {
                            $dataW.textContent = dataW;
                            $dataX.textContent = dataX;
                            $dataY.textContent = dataY;
                            $dataZ.textContent = dataZ;
                        });
                    });

                    connection.start()
                        .then(function () {
                            setSocketConnectionFeedback(connection.connectionState);
                            //connection.invoke('RequestInitialState').catch((err) => {
                            //    return console.error('Could not request initial state', err);
                            //});
                        })
                        .catch(function (err) {
                            setSocketConnectionFeedback('Error ' + err.toString());
                            return console.error(err.toString());
                        });

                    connection.onclose(function() {
                        console.log('connection onclose');
                        setSocketConnectionFeedback(connection.connectionState);
                    });

                    connection.onreconnecting(function(error) {
                        console.log('connection onreconnecting', error);
                        setSocketConnectionFeedback(connection.connectionState);
                    });

                    connection.onreconnected(function() {
                        console.log('connection onreconnected');
                        setSocketConnectionFeedback(connection.connectionState);

                        connection.invoke('RequestInitialState').catch((err) => {
                            return console.error('Could not request initial state', err);
                        });
                    });

                    function setSocketConnectionFeedback(message) {
                        document.getElementById('socketStatus').textContent = message;
                    }

                    </script>
                </body>
                </html>
                ";


            return new ContentResult
            {
                Content = data,
                ContentType = "text/html",
                StatusCode = 200
            };
        }
    }

}
