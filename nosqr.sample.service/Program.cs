using nosqr.api.Services;
using sample.domain;
using sample.domain.cqrs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nosqr.sample.service
{
    class Program
    {
        static void Main(string[] args)
        {

            var bus = new FileEventService("c:\\bus\\");
            System.Console.ReadLine();

            
        }   
    }
}
