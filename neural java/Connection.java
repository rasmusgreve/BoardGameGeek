package itu.ejuu.neural;

public class Connection {
	
	private Node fromNode, toNode;
	private double weight;

	public Connection(double weight, Node fromNode, Node toNode){
		this.weight = weight;
		this.fromNode = fromNode;
		this.toNode = toNode;
		
		// adding to nodes
		fromNode.conn_out.add(this);
		toNode.conn_in.add(this);
	}
	
	public double getWeight(){
		return weight;
	}
	
	public void changeWeight(double amount){
		weight += amount;
	}
	
	public Node getFromNode(){
		return fromNode;
	}
	
	public Node getToNode(){
		return toNode;
	}

	public void updateWeight() {
		double delta = Node.L*toNode.getError()*fromNode.getOutput();
		this.changeWeight(delta);
	}
}
