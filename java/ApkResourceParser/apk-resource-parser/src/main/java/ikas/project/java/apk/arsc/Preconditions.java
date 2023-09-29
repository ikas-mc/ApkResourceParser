package ikas.project.java.apk.arsc;

public class Preconditions {
    public static void checkArgument(boolean expression, /*@CheckForNull*/ Object errorMessage) {
        if (!expression) {
            var message = switch (errorMessage) {
                case null -> "";
                case String s -> s;
                case Object o -> o.toString();
            };
            throw new IllegalArgumentException(message);
        }
    }

    public static void checkState(boolean expression) {
        if (!expression) {
            throw new IllegalStateException();
        }
    }

    public static void checkState(boolean expression, /*@CheckForNull*/ Object errorMessage) {
        if (!expression) {
            var message = switch (errorMessage) {
                case null -> "";
                case String s -> s;
                case Object o -> o.toString();
            };
            throw new IllegalStateException(message);
        }
    }

    public static <T> T checkNotNull(/*@CheckForNull*/ T reference) {
        if (reference == null) {
            throw new NullPointerException();
        }
        return reference;
    }

    public static <T> T checkNotNull(/*@CheckForNull*/ T reference, /*@CheckForNull*/ Object errorMessage) {
        if (reference == null) {
            var message = switch (errorMessage) {
                case null -> "";
                case String s -> s;
                case Object o -> o.toString();
            };
            throw new NullPointerException(message);
        }
        return reference;
    }

    public static <T> T checkNotNull(
            /*@CheckForNull*/ T reference,
                              String errorMessageTemplate,
            /*@CheckForNull*/ /*@Nullable*/ Object... errorMessageArgs) {
        if (reference == null) {
            throw new NullPointerException(String.format(errorMessageTemplate, errorMessageArgs));
        }
        return reference;
    }

}
