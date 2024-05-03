export function sort(values: string[][]): string[] {
    let stack: string[] = [];
    let visited: Record<string, boolean> = {};

    let graph: Record<string, string[]> = {};
    for (let [a, b] of values) {
        if (!graph[a]) graph[a] = [];
        graph[a].push(b);
    }

    function visit(node: string) {
        if (visited[node]) return;
        visited[node] = true;

        let neighbors = graph[node] || [];
        for (let neighbor of neighbors) {
            visit(neighbor);
        }

        stack.push(node);
    }

    for (let node in graph) {
        visit(node);
    }

    stack.reverse();

    return stack;
}
