function decodeBase64(str) {
    try {
        return decodeURIComponent(escape(atob(str)));
    } catch {
        return str; // fallback si algo falla
    }
}