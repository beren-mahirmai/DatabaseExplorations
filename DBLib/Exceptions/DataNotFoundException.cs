using System;

namespace DBLib.Exceptions {

    // This is a special exception that is thrown when requested data is not found, allowing the
    // caller to recover by searching other locations, such as a data file, for the data.
    public class DataNotFoundException : Exception {
        
        private String RequestedKey;

        public DataNotFoundException(String requestedKey) {
            RequestedKey = requestedKey;
        }
    }
}