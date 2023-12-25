export interface NodeConnection {
	from: ConnectedProperty;
	to: ConnectedProperty;
}

export interface ConnectedProperty {
	instanceId: string;
	propertyId: string;
}
