/* 
Perfmonport perfmonCollectCounterData sample script, using DotNet Core HttpClient.

Demonstrates retrieving/parsing the performance counter data from a specific
object, in this case all Hunt Pilots.

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
 
namespace perfmonCollectCounterData {
    public class Program
    {
        static async Task Main()
        {
            Console.WriteLine( "\nStarting up...\n" );

            // Load environment variables from .env
            DotNetEnv.Env.Load( "../../.env" );

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
                requestMessage.RequestUri = new Uri( $"https://{ System.Environment.GetEnvironmentVariable( "CUCM_ADDRESS" ) }:8443/perfmonservice2/services/PerfmonService" );

                var credentials = System.Environment.GetEnvironmentVariable( "USERNAME" ) + ":" + System.Environment.GetEnvironmentVariable( "PASSWORD" );
                var credentialsBase64  = Convert.ToBase64String( System.Text.ASCIIEncoding.ASCII.GetBytes( "Administrator:ciscopsdt" ) );
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue( "Basic", credentialsBase64 );

                requestMessage.Headers.Add( "SOAPAction", "SOAPAction: \"perfmonCollectCounterData\"" );

                String xmlBody = $@"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soap=""http://schemas.cisco.com/ast/soap"">
                    <soapenv:Header/>
                    <soapenv:Body>
                        <soap:perfmonCollectCounterData>
                            <soap:Host>{ System.Environment.GetEnvironmentVariable( "CUCM_ADDRESS" ) }</soap:Host>
                            <soap:Object>Cisco Hunt Pilots</soap:Object>
                        </soap:perfmonCollectCounterData>
                    </soapenv:Body>
                </soapenv:Envelope>";
                requestMessage.Content = new StringContent( xmlBody, Encoding.UTF8, "text/xml" );

                if ( debug ) {

                    Console.WriteLine( "perfmonCollectCounterData Request:" );
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

            Console.WriteLine( "perfmonCollectCounterData result: SUCCESS" );
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

    } //class

} //namespace