using System;

namespace crafting_interpreters {
    public class Return : Exception {
        public object value;

        public Return(object value) : base() {
            this.value = value;
        }
    }
}