using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DocumentSimilarity.BLL
{
    public class Timer
    {
        
    
      private  DateTime start;
      private DateTime end;
      private double intervall = 0;
        Boolean start_flag=false;
        Boolean stop_flag=false;
        public void Start()
        {
            stop_flag = false;
            if (start_flag)
            {
                throw new MethodAccessException("the timer is already started. please stop it before start again!"); 
            }
            else { start = System.DateTime.Now; start_flag = true; }
            
        }
        public double Stop()
        {
            double interval = 0;
            start_flag = false;
           if (stop_flag)
            {
                throw new MethodAccessException("the timer is already stoped. please start it before stop again!");
            }
           else { end = System.DateTime.Now;

             interval= Math.Round((end - start).TotalSeconds, 2);
             stop_flag = true;
             
           }
           return interval;
        }

        
    }
    
}