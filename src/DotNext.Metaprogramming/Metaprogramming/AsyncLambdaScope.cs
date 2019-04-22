﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DotNext.Metaprogramming
{
    using Runtime.CompilerServices;
    using static Reflection.DelegateType;

    internal sealed class AsyncLambdaScope<D>: LambdaScope, IExpressionBuilder<Expression<D>>, ICompoundStatement<Action<LambdaContext>>
        where D: Delegate
    {
        private ParameterExpression recursion;
        private readonly TaskType taskType;

        internal AsyncLambdaScope(LexicalScope parent = null)
            : base(parent, false)
        {
            if (typeof(D).IsAbstract)
                throw new GenericArgumentException<D>(ExceptionMessages.AbstractDelegate, nameof(D));
            var invokeMethod = GetInvokeMethod<D>();
            taskType = new TaskType(invokeMethod.ReturnType);
            Parameters = GetParameters(invokeMethod.GetParameters());
        }

        void ICompoundStatement<Action<LambdaContext>>.ConstructBody(Action<LambdaContext> body)
        {
            using(var context = new LambdaContext(this))
                body(context);
        }

        /// <summary>
        /// Gets this lambda expression suitable for recursive call.
        /// </summary>
        internal override Expression Self
        {
            get
            {
                if (recursion is null)
                    recursion = Expression.Variable(typeof(D), "self");
                return recursion;
            }
        }

        /// <summary>
        /// The list lambda function parameters.
        /// </summary>
        internal override IReadOnlyList<ParameterExpression> Parameters { get; }

        internal override Expression Return(Expression result) => new AsyncResultExpression(result, taskType);

        public Expression<D> Build()
        {
            var body = base.Build();
            if (body.Type != taskType)
                body = body.AddEpilogue(true, new AsyncResultExpression(taskType));
            Expression<D> lambda;
            using(var builder = new Runtime.CompilerServices.AsyncStateMachineBuilder<D>(Parameters))
            {
                lambda = builder.Build(body, tailCall);
            }
            //build lambda expression
            if (!(recursion is null))
                lambda = Expression.Lambda<D>(Expression.Block(Sequence.Singleton(recursion),
                    Expression.Assign(recursion, lambda),
                    Expression.Invoke(recursion, Parameters)), Parameters);
            return lambda;
        }

        public override void Dispose()
        {
            recursion = null;
            base.Dispose();
        }
    }
}
