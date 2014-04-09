package itu.ejuu.neural;

import java.util.ArrayList;

public class Node {
	
	public static double L = 1.0; // learning rate
	
	public ArrayList<Connection> conn_in, conn_out;
	private double inputValue = 0.0;
	private double outputValue = 0.0;
	private double errorValue = 0.0;
	private double bias = 1.0;

	public Node(double inititalBias){
		bias = inititalBias;
		conn_in = new ArrayList<Connection>();
		conn_out = new ArrayList<Connection>();
	}
	
	public void setInput(double input){
		this.inputValue = input;
	}
	
	public double getOutput(){
		return outputValue;
	}
	
	public double getError(){
		return errorValue;
	}
	
	public double forwardOperation(){
		if(conn_in.size() == 0){ // input node
			outputValue = inputValue;
			return inputValue;
		}
		
		double S = bias;
		for(Connection c : conn_in){
			S += c.getFromNode().forwardOperation() * c.getWeight();
		}
		
		double a = 1.0/(1.0 + Math.pow(Math.E, -1.0 * S));
		outputValue = a;
		return a;
	}
	
	public void computeError(double T){
		double O = this.outputValue;
		
		if(conn_out.size() == 0){ // output node
			errorValue = O*(1-O)*(T - O);
			return;
		}
		// layer node (warning: requires next layer to be calculated first
		double nextSum = 0.0;
		for(Connection c : conn_out){
			nextSum += c.getToNode().getError() * c.getWeight();
		}
		errorValue = O*(1-O) * nextSum;
	}
	
	public void updateBias(){
		double delta = Node.L*this.getError();
		bias = bias + delta;
	}

	public double getBias() {
		return bias;
	}
}
