using Lime.Protocol;
using System;

namespace Calculon.Models
{
    public class User
    {
        public Node Node { get; set; }
        public Guid Id { get; set; }
        public Session Session { get; set; }
        public double FirstNumber { get; set; }
        public double SecondNumber { get; set; }
        public int Operation { get; set; }

        public User()
        {
            Id = Guid.NewGuid();
        }
    }
}