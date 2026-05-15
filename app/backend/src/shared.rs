use std::sync::Arc;

use arc_swap::ArcSwap;

#[derive(Clone)]
pub struct Shared<T>(Arc<ArcSwap<T>>);

impl<T> Shared<T> {
    pub fn new(value: T) -> Self {
        Self(Arc::new(ArcSwap::from_pointee(value)))
    }

    pub fn get(&self) -> T
    where
        T: Clone,
    {
        (**self.0.load()).clone()
    }

    pub fn set(&self, value: T) {
        self.0.store(Arc::new(value));
    }
}
