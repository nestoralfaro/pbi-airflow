from airflow import DAG
from datetime import datetime, timedelta
from airflow.operators.bash import BashOperator

default_args = {
    "owner": "nestor",
    "retries": 5,
    "retry_delay": timedelta(minutes=2)
}

with DAG(
    dag_id="scenario_two_v3",
    default_args=default_args,
    description="executes C# program with dotnet",
    start_date=datetime(2024,5,1),
    schedule_interval ="@daily"
) as dag:
    task1 = BashOperator(
        task_id="first_task",
        bash_command="echo starting workflow"
    )
    task2 = BashOperator(
        task_id="second_task",
        bash_command="dotnet run --project /Users/nestoralfaro/Desktop/pbi-scenario-two/dotnetapp/dotnetapp.csproj"
    )

    task1.set_downstream(task2)
