import flask
from flask_cors import CORS
import json
import numpy as np
import pickle

# Initializing flask app
app = flask.Flask(__name__)
# Adding cors to flask
CORS(app)


# Controller-1
@app.route("/demo", methods=['GET'])
def get_demo():
    return "This is a demo api"


# Controller-2
@app.route("/depression_prediction", methods=['POST'])
def get_demo_name():
    model = pickle.load(open("depresson_gpr_model.pkl", 'rb'))

    data = flask.request.data
    post_data = json.loads(data)

    # Extract features in the correct order
    feature_order = [
        "mean_entropy", "entropy_Landmark12", "entropy_Landmark13",
        "entropy_Landmark14", "entropy_Landmark15", "entropy_Landmark16",
        "entropy_Landmark23", "entropy_Landmark24", "entropy_Landmark25",
        "entropy_Landmark26", "entropy_Landmark27", "entropy_Landmark28",
        "entropy_Landmark29", "aggregated_entropy"
    ]
    
    # Convert the feature values to a numpy array
    input_data = np.array([[post_data[feature] for feature in feature_order]])

    # Perform prediction using the loaded scikit-learn model
    result = model.predict(input_data)

    # Return the prediction result as a JSON response
    return json.dumps({"result": result.tolist()})


# Running the api
if __name__ == '__main__':
    app.run()