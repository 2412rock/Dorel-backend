﻿namespace DorelAppBackend.Models.Requests
{
    public class AssignRequest
    {
        public int ServiciuId { get; set; }
        public int[] JudeteIds { get; set; }
        public Imagine[] Imagini { get; set; }
        public string Descriere { get; set; }
        public bool Ofer { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}
