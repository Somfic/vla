use std::collections::HashMap;

pub fn sort(nodes: Vec<String>, connections: Vec<(String, String)>) -> Vec<String> {
    let mut graph: HashMap<String, Vec<String>> = HashMap::new();
    let mut in_degree: HashMap<String, usize> = HashMap::new();

    for node in &nodes {
        graph.insert(node.clone(), vec![]);
        in_degree.insert(node.clone(), 0);
    }

    for (src, dst) in connections {
        if let Some(neighbors) = graph.get_mut(&src) {
            neighbors.push(dst.clone());
        }
        *in_degree.entry(dst).or_insert(0) += 1;
    }

    let mut queue: Vec<String> = in_degree
        .iter()
        .filter_map(|(node, &deg)| if deg == 0 { Some(node.clone()) } else { None })
        .collect();

    let mut sorted = vec![];

    while let Some(node) = queue.pop() {
        sorted.push(node.clone());
        if let Some(neighbors) = graph.get(&node) {
            for neighbor in neighbors {
                if let Some(deg) = in_degree.get_mut(neighbor) {
                    *deg -= 1;
                    if *deg == 0 {
                        queue.push(neighbor.clone());
                    }
                }
            }
        }
    }

    if sorted.len() != nodes.len() {
        panic!("Topological sort failed: the graph may contain a cycle or reference missing nodes");
    }

    sorted
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_topological_sort() {
        let nodes = vec!["a".into(), "b".into(), "c".into()];
        let connections = vec![("a".into(), "b".into()), ("b".into(), "c".into())];

        let sorted = sort(nodes, connections);
        assert_eq!(sorted, vec!["a", "b", "c"]);
    }

    #[test]
    #[should_panic(
        expected = "Topological sort failed: the graph may contain a cycle or reference missing nodes"
    )]
    fn test_cycle_detection() {
        let nodes = vec!["a".into(), "b".into(), "c".into()];
        let connections = vec![
            ("a".into(), "b".into()),
            ("b".into(), "c".into()),
            ("c".into(), "a".into()),
        ];
        sort(nodes, connections);
        // This point should never be reached
        panic!("Test failed: Cycle was not detected");
    }

    #[test]
    fn test_disconnected_graph() {
        let nodes = vec!["a".into(), "b".into(), "c".into(), "d".into()];
        let connections = vec![("a".into(), "b".into()), ("c".into(), "d".into())];
        let sorted = sort(nodes, connections);
        // Valid outputs could be ["a", "b", "c", "d"] or ["c", "d", "a", "b"] or other variations
        assert_eq!(sorted.len(), 4);
        // make sure a is before b and c is before d
        assert!(sorted.iter().position(|x| x == "a") < sorted.iter().position(|x| x == "b"));
        assert!(sorted.iter().position(|x| x == "c") < sorted.iter().position(|x| x == "d"));
    }
}
