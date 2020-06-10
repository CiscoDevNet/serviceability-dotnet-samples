/* 
Risport selectCmDeviceExt sample script, using DotNet Core HttpClient.

Filters by device name, passing in two specific device names to query.

Note: selectCmDeviceExt does not support the '*' wildcard in the filter.

Copyright (c) 2020 Cisco and/or its affiliates.
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using DotNetEnv;
 
public class serviceabilityDotnetExamples
{
    static async Task Main()
    {
        Console.WriteLine( "\nStarting up...\n" );

        // Load environment variables from .env
        DotNetEnv.Env.Load( ".env" );

        Boolean debug = System.Environment.GetEnvironmentVariable( "DEBUG" ) == "True";

        var customHandler = new HttpClientHandler();

        // Disables HTTPS certificate checking - TODO implement real cert check in production
        customHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        customHandler.CookieContainer = new CookieContainer();
        customHandler.UseCookies = true; // Use cookies to reuse authentication for performance
        customHandler.PreAuthenticate = true; // Send Basic auth first, avoid challenge/response sequence

        // Creat client using custom handler
        HttpClient client = new HttpClient( customHandler );
        client.DefaultRequestHeaders.ConnectionClose = false; // Reuse connection for performance

        // Wrap in using to ensure proper dispose of resources
        using  ( var requestMessage = new HttpRequestMessage() ) {

            requestMessage.Method = HttpMethod.Post;
            requestMessage.RequestUri = new Uri( "https://ds-ucm1251.cisco.com:8443/realtimeservice2/services/RISService70" );

            var credentials  = Convert.ToBase64String( System.Text.ASCIIEncoding.ASCII.GetBytes( "Administrator:ciscopsdt" ) );
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue( "Basic", credentials );

            requestMessage.Headers.Add( "SOAPAction", "SOAPAction: \"selectCmDeviceExt\"" );

            String xmlBody = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://schemas.cisco.com/ast/soap"">
                <soapenv:Header/>
                <soapenv:Body>
                    <soap:selectCmDeviceExt>
                        <soap:StateInfo></soap:StateInfo>
                        <soap:CmSelectionCriteria>
                            <soap:MaxReturnedDevices>1000</soap:MaxReturnedDevices>
                            <soap:DeviceClass>Any</soap:DeviceClass>
                            <soap:Model>255</soap:Model>
                            <soap:Status>Any</soap:Status>
                            <soap:NodeName></soap:NodeName>
                            <soap:SelectBy>Name</soap:SelectBy>
                            <soap:SelectItems>
                            <soap:item>
                                <soap:Item>SEP001EF727852D</soap:Item>
                            </soap:item>
                            <soap:item>
                                <soap:Item>IPCMRAEU5UCM5X7</soap:Item>
                            </soap:item>
                            </soap:SelectItems>
                            <soap:Protocol>Any</soap:Protocol>
                            <soap:DownloadStatus>Any</soap:DownloadStatus>
                        </soap:CmSelectionCriteria>
                    </soap:selectCmDeviceExt>
                </soapenv:Body>
                </soapenv:Envelope>";
            requestMessage.Content = new StringContent( xmlBody, Encoding.UTF8, "text/xml" );

            if ( debug ) {

                Console.WriteLine( "selectCmDevice Request:" );
                Console.WriteLine( requestMessage.ToString());
                Console.WriteLine( prettyPrint( await requestMessage.Content.ReadAsStringAsync() ) );
                Console.WriteLine();            
            }

            HttpResponseMessage reply = await client.SendAsync( requestMessage );

            if ( debug ) {

                Console.WriteLine( "Response:" );
                Console.WriteLine( reply.ToString() );
                if ( reply.Content != null )
                {
                    Console.WriteLine( prettyPrint( await reply.Content.ReadAsStringAsync() ) );
                }
                Console.WriteLine();            
            }

        // Throws an exception if result is non-200 range
        reply.EnsureSuccessStatusCode();

        Console.WriteLine( "selectCmDeviceExt result: SUCCESS" );
        }
    }
    private static string prettyPrint( string xml ) {
        try
        {
            XDocument doc = XDocument.Parse( xml );
            return doc.ToString();
        }
        catch ( Exception )
        {
            return xml;
        }
    }
}